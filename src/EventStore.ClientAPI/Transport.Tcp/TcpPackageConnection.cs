﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using EventStore.ClientAPI.Common.Utils;
using EventStore.ClientAPI.SystemData;
using Microsoft.Extensions.Logging;

namespace EventStore.ClientAPI.Transport.Tcp
{
  internal class TcpPackageConnection
  {
    private static readonly TcpClientConnector Connector = new TcpClientConnector();

    public bool IsClosed { get { return _connection.IsClosed; } }
    public int SendQueueSize { get { return _connection.SendQueueSize; } }
    public IPEndPoint RemoteEndPoint { get { return _connection.RemoteEndPoint; } }
    public IPEndPoint LocalEndPoint { get { return _connection.LocalEndPoint; } }
    public readonly Guid ConnectionId;

    private static readonly ILogger _log = TraceLogger.GetLogger<TcpPackageConnection>();
    private readonly Action<TcpPackageConnection, TcpPackage> _handlePackage;
    private readonly Action<TcpPackageConnection, Exception> _onError;

    private readonly LengthPrefixMessageFramer _framer;
    private readonly ITcpConnection _connection;

    public TcpPackageConnection(IPEndPoint remoteEndPoint,
                                Guid connectionId,
                                bool ssl,
                                string targetHost,
                                bool validateServer,
                                TimeSpan timeout,
                                Action<TcpPackageConnection, TcpPackage> handlePackage,
                                Action<TcpPackageConnection, Exception> onError,
                                Action<TcpPackageConnection> connectionEstablished,
                                Action<TcpPackageConnection, SocketError> connectionClosed)
    {
      Ensure.NotNull(remoteEndPoint, "remoteEndPoint");
      Ensure.NotEmptyGuid(connectionId, "connectionId");
      Ensure.NotNull(handlePackage, "handlePackage");
      if (ssl)
        Ensure.NotNullOrEmpty(targetHost, "targetHost");

      ConnectionId = connectionId;
      _handlePackage = handlePackage;
      _onError = onError;

      //Setup callback for incoming messages
      _framer = new LengthPrefixMessageFramer();
      _framer.RegisterMessageArrivedCallback(IncomingMessageArrived);

      var connectionCreated = new ManualResetEventSlim();
      // ReSharper disable ImplicitlyCapturedClosure
      _connection = Connector.ConnectTo(
          connectionId,
          remoteEndPoint,
          ssl,
          targetHost,
          validateServer,
          timeout,
          tcpConnection =>
          {
            connectionCreated.Wait();
            if (_log.IsDebugLevelEnabled())
            {
              _log.LogDebug("TcpPackageConnection: connected to [{0}, L{1}, {2:B}].", tcpConnection.RemoteEndPoint, tcpConnection.LocalEndPoint, connectionId);
            }
            connectionEstablished?.Invoke(this);
          },
          (conn, error) =>
          {
            connectionCreated.Wait();
            if (_log.IsDebugLevelEnabled())
            {
              _log.LogDebug("TcpPackageConnection: connection to [{0}, L{1}, {2:B}] failed. Error: {3}.", conn.RemoteEndPoint, conn.LocalEndPoint, connectionId, error);
            }
            connectionClosed?.Invoke(this, error);
          },
          (conn, error) =>
          {
            connectionCreated.Wait();
            if (_log.IsDebugLevelEnabled())
            {
              _log.LogDebug("TcpPackageConnection: connection [{0}, L{1}, {2:B}] was closed {3}", conn.RemoteEndPoint, conn.LocalEndPoint,
                            ConnectionId, error == SocketError.Success ? "cleanly." : "with error: " + error + ".");
            }

            connectionClosed?.Invoke(this, error);
          });
      // ReSharper restore ImplicitlyCapturedClosure

      connectionCreated.Set();
    }

    private void OnRawDataReceived(ITcpConnection connection, IEnumerable<ArraySegment<byte>> data)
    {
      try
      {
        _framer.UnFrameData(data);
      }
      catch (PackageFramingException exc)
      {
        _log.LogError(exc, "TcpPackageConnection: [{0}, L{1}, {2:B}]. Invalid TCP frame received.", RemoteEndPoint, LocalEndPoint, ConnectionId);
        Close("Invalid TCP frame received.");
        return;
      }

      //NOTE: important to be the last statement in the callback
      connection.ReceiveAsync(OnRawDataReceived);
    }

    private void IncomingMessageArrived(ArraySegment<byte> data)
    {
      var package = new TcpPackage();
      var valid = false;
      try
      {
        package = TcpPackage.FromArraySegment(data);
        valid = true;
        _handlePackage(this, package);
      }
      catch (Exception e)
      {
        _connection.Close(string.Format("Error when processing TcpPackage {0}: {1}",
                                        valid ? package.Command.ToString() : "<invalid package>", e.Message));

        var message = string.Format("TcpPackageConnection: [{0}, L{1}, {2}] ERROR for {3}. Connection will be closed.",
                                    RemoteEndPoint, LocalEndPoint, ConnectionId,
                                    valid ? package.Command.ToString() : "<invalid package>");
        _onError?.Invoke(this, e);
        if (_log.IsDebugLevelEnabled()) _log.LogDebug(e, message);
      }
    }

    public void StartReceiving()
    {
      if (_connection == null)
      {
        throw new InvalidOperationException("Failed connection.");
      }

      _connection.ReceiveAsync(OnRawDataReceived);
    }

    public void EnqueueSend(in TcpPackage package)
    {
      if (_connection == null)
      {
        throw new InvalidOperationException("Failed connection.");
      }

      _connection.EnqueueSend(_framer.FrameData(package.AsArraySegment()));
    }

    public void Close(string reason)
    {
      if (_connection == null)
      {
        throw new InvalidOperationException("Failed connection.");
      }

      _connection.Close(reason);
    }
  }
}