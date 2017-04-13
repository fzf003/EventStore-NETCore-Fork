﻿using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using ProtoBuf;

namespace EventStore.Core.Services.Transport.Tcp
{
  public static class ProtobufExtensions
  {
    private static readonly ConcurrentStack<MemoryStream> _streams;

    static ProtobufExtensions()
    {
      _streams = new ConcurrentStack<MemoryStream>();
      for (var i = 0; i < 300; i++)
      {
        _streams.Push(new MemoryStream(2048));
      }
    }

    private static readonly ILogger Log = TraceLogger.GetLogger(typeof(ProtobufExtensions));

    static MemoryStream AcquireStream()
    {
      for (var i = 0; i < 1000; i++)
      {
        if (_streams.TryPop(out MemoryStream ret))
        {
          ret.SetLength(0);
          return ret;
        }
        if ((i + 1) % 5 == 0)
        {
          Thread.Sleep(1); //need to do better than this
        }
      }
      throw new UnableToAcquireStreamException();
    }

    static void ReleaseStream(MemoryStream stream)
    {
      _streams.Push(stream);
    }

    public static T Deserialize<T>(this byte[] data)
    {
      return Deserialize<T>(new ArraySegment<byte>(data));
    }

    public static T Deserialize<T>(this ArraySegment<byte> data)
    {
      try
      {
        using (var memory = new MemoryStream(data.Array, data.Offset, data.Count)) //uses original buffer as memory
        {
          var res = Serializer.Deserialize<T>(memory);
          return res;
        }
      }
      catch (Exception e)
      {
        if (Log.IsInformationLevelEnabled()) Log.LogInformation(e, "Deserialization to {0} failed", typeof(T).FullName);
        return default(T);
      }
    }

    public static ArraySegment<byte> Serialize<T>(this T protoContract)
    {
      MemoryStream stream = null;
      try
      {
        stream = AcquireStream();
        Serializer.Serialize(stream, protoContract);
        var res = new ArraySegment<byte>(stream.ToArray(), 0, (int)stream.Length);
        return res;
      }
      finally
      {
        if (stream != null)
        {
          ReleaseStream(stream);
        }
      }
    }

    public static byte[] SerializeToArray<T>(this T protoContract)
    {
      MemoryStream stream = null;
      try
      {
        stream = AcquireStream();
        Serializer.Serialize(stream, protoContract);
        return stream.ToArray();
      }
      finally
      {
        if (stream != null)
        {
          ReleaseStream(stream);
        }
      }
    }
  }
}
