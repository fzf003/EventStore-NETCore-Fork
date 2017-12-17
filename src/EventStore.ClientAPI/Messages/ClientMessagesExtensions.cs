﻿using System;
using System.Net;
using CuteAnt;
using CuteAnt.Collections;
using CuteAnt.Reflection;
using EventStore.ClientAPI.Internal;
using EventStore.ClientAPI.Serialization;
using Microsoft.Extensions.Logging;

namespace EventStore.ClientAPI.Messages
{
  internal static partial class ClientMessage
  {
    private static ILogger s_logger = TraceLogger.GetLogger(typeof(ClientMessage));

    #region -- class NotHandled --

    public partial class NotHandled
    {
      public partial class MasterInfo
      {
        public IPEndPoint ExternalTcpEndPoint { get { return new IPEndPoint(IPAddress.Parse(ExternalTcpAddress), ExternalTcpPort); } }

        public IPEndPoint ExternalSecureTcpEndPoint
        {
          get
          {
            return ExternalSecureTcpAddress == null || ExternalSecureTcpPort == null
                    ? null
                    : new IPEndPoint(IPAddress.Parse(ExternalSecureTcpAddress), ExternalSecureTcpPort.Value);
          }
        }

        public IPEndPoint ExternalHttpEndPoint { get { return new IPEndPoint(IPAddress.Parse(ExternalHttpAddress), ExternalHttpPort); } }
      }
    }

    #endregion

    #region == ToRawRecordedEvent ==

    internal static RecordedEvent ToRawRecordedEvent(this ClientMessage.EventRecord systemRecord)
    {
      return new RecordedEvent(
        systemRecord.EventStreamId,
        new Guid(systemRecord.EventId),
        systemRecord.EventNumber,
        systemRecord.EventType,
        systemRecord.Created,
        systemRecord.CreatedEpoch,
        systemRecord.Data ?? Empty.ByteArray,
        systemRecord.Metadata ?? Empty.ByteArray,
        systemRecord.DataContentType == 1);
    }

    #endregion

    #region == ToRawResolvedEvent ==

    internal static ClientAPI.ResolvedEvent ToRawResolvedEvent(this ClientMessage.ResolvedEvent evnt)
    {
      return new ClientAPI.ResolvedEvent(
          evnt.Event?.ToRawRecordedEvent(),
          evnt.Link?.ToRawRecordedEvent(),
          new Position(evnt.CommitPosition, evnt.PreparePosition));
    }

    internal static ClientAPI.ResolvedEvent ToRawResolvedEvent(this ClientMessage.ResolvedIndexedEvent evnt)
    {
      return new ClientAPI.ResolvedEvent(evnt.Event?.ToRawRecordedEvent(), evnt.Link?.ToRawRecordedEvent(), null);
    }

    internal static ClientAPI.ResolvedEvent? ToRawResolvedEvent(this ClientMessage.ResolvedIndexedEvent evnt, EventReadStatus readStatus)
    {
      return readStatus == EventReadStatus.Success
            ? new ClientAPI.ResolvedEvent(evnt.Event?.ToRawRecordedEvent(), evnt.Link?.ToRawRecordedEvent(), null)
            : default(ClientAPI.ResolvedEvent?);
    }

    #endregion

    #region == ToRawResolvedEvents ==

    internal static ClientAPI.ResolvedEvent[] ToRawResolvedEvents(this ClientMessage.ResolvedEvent[] events)
    {
      if (events == null || events.Length == 0)
      {
        return Empty.ResolvedEvents;
      }
      else
      {
        var result = new ClientAPI.ResolvedEvent[events.Length];
        for (int i = 0; i < result.Length; ++i)
        {
          result[i] = events[i].ToRawResolvedEvent();
        }
        return result;
      }
    }

    internal static ClientAPI.ResolvedEvent[] ToRawResolvedEvents(this ClientMessage.ResolvedIndexedEvent[] events)
    {
      if (events == null || events.Length == 0)
      {
        return Empty.ResolvedEvents;
      }
      else
      {
        var result = new ClientAPI.ResolvedEvent[events.Length];
        for (int i = 0; i < result.Length; ++i)
        {
          result[i] = events[i].ToRawResolvedEvent();
        }
        return result;
      }
    }

    #endregion

    #region ** ToRecordedEvent **

    private static RecordedEvent<object> ToRecordedEvent(this ClientMessage.EventRecord systemRecord)
    {
      try
      {
        return new RecordedEvent<object>(
          systemRecord.EventStreamId,
          new Guid(systemRecord.EventId),
          systemRecord.EventNumber,
          systemRecord.EventType,
          systemRecord.Created,
          systemRecord.CreatedEpoch,
          SerializationManager.DeserializeEvent(systemRecord.Metadata, systemRecord.Data),
          systemRecord.DataContentType == 1);
      }
      catch (Exception exc)
      {
        s_logger.LogWarning(exc,
            $"Can't deserialize the recorded event: StreamId - {systemRecord.EventStreamId}, EventId - {systemRecord.EventId}, EventNumber - {systemRecord.EventNumber}, EventType - {systemRecord.EventType}");
        return new RecordedEvent<object>(
          systemRecord.EventStreamId,
          new Guid(systemRecord.EventId),
          systemRecord.EventNumber,
          systemRecord.EventType,
          systemRecord.Created,
          systemRecord.CreatedEpoch,
          DefaultFullEvent.Null,
          systemRecord.DataContentType == 1);
      }
    }

    #endregion

    #region ** ToRecordedEvent<T> **

    private static RecordedEvent<T> ToRecordedEvent<T>(this ClientMessage.EventRecord systemRecord) where T : class
    {
      try
      {
        return new RecordedEvent<T>(
          systemRecord.EventStreamId,
          new Guid(systemRecord.EventId),
          systemRecord.EventNumber,
          systemRecord.EventType,
          systemRecord.Created,
          systemRecord.CreatedEpoch,
          SerializationManager.DeserializeEvent<T>(systemRecord.Metadata, systemRecord.Data),
          systemRecord.DataContentType == 1);
      }
      catch (Exception exc)
      {
        s_logger.LogWarning(exc,
            $"Can't deserialize the recorded event: StreamId - {systemRecord.EventStreamId}, EventId - {systemRecord.EventId}, EventNumber - {systemRecord.EventNumber}, EventType - {systemRecord.EventType}");
        return new RecordedEvent<T>(
          systemRecord.EventStreamId,
          new Guid(systemRecord.EventId),
          systemRecord.EventNumber,
          systemRecord.EventType,
          systemRecord.Created,
          systemRecord.CreatedEpoch,
          DefaultFullEvent<T>.Null,
          systemRecord.DataContentType == 1);
      }
    }

    private static RecordedEvent<T> ToRecordedEvent<T>(this ClientMessage.EventRecord systemRecord, EventMetadata metadata, Type eventType) where T : class
    {
      try
      {
        return new RecordedEvent<T>(
          systemRecord.EventStreamId,
          new Guid(systemRecord.EventId),
          systemRecord.EventNumber,
          systemRecord.EventType,
          systemRecord.Created,
          systemRecord.CreatedEpoch,
          SerializationManager.DeserializeEvent<T>(metadata, eventType, systemRecord.Data),
          systemRecord.DataContentType == 1);
      }
      catch (Exception exc)
      {
        s_logger.LogWarning(exc,
            $"Can't deserialize the recorded event: StreamId - {systemRecord.EventStreamId}, EventId - {systemRecord.EventId}, EventNumber - {systemRecord.EventNumber}, EventType - {systemRecord.EventType}");
        return new RecordedEvent<T>(
          systemRecord.EventStreamId,
          new Guid(systemRecord.EventId),
          systemRecord.EventNumber,
          systemRecord.EventType,
          systemRecord.Created,
          systemRecord.CreatedEpoch,
          DefaultFullEvent<T>.Null,
          systemRecord.DataContentType == 1);
      }
    }

    #endregion

    #region ** IResolvedEventDeserializer cache **

    private static readonly DictionaryCache<Type, IResolvedEventDeserializer> s_eventDeserializerCache =
        new DictionaryCache<Type, IResolvedEventDeserializer>(DictionaryCacheConstants.SIZE_MEDIUM);
    private static readonly Func<Type, IResolvedEventDeserializer> s_createEventDeserializer = CreateEventDeserializer;

    private static IResolvedEventDeserializer CreateEventDeserializer(Type eventType)
    {
      var deserializerType = typeof(ResolvedEventDeserializer<>).GetCachedGenericType(eventType);
      return ActivatorUtils.FastCreateInstance<IResolvedEventDeserializer>(deserializerType);
    }

    #endregion

    #region == ToResolvedEvent2 ==

    internal static IResolvedEvent2 ToResolvedEvent2(this ClientMessage.ResolvedEvent evnt)
    {
      try
      {
        var systemRecord = evnt.Event;
        var eventMeta = systemRecord != null ? SerializationManager.DeserializeMetadata(systemRecord.Metadata) : null;
        systemRecord = evnt.Event;
        var linkMeta = systemRecord != null ? SerializationManager.DeserializeMetadata(systemRecord.Metadata) : null;

        var eventType = TypeUtils.ResolveType((linkMeta ?? eventMeta).EventType);
        var deserializer = s_eventDeserializerCache.GetItem(eventType, s_createEventDeserializer);
        return deserializer.ToResolvedEvent(evnt, eventMeta, linkMeta, eventType);
      }
      catch { return evnt.ToResolvedEvent(); }
    }

    internal static IResolvedEvent2 ToResolvedEvent2(this ClientMessage.ResolvedIndexedEvent evnt)
    {
      try
      {
        var systemRecord = evnt.Event;
        var eventMeta = systemRecord != null ? SerializationManager.DeserializeMetadata(systemRecord.Metadata) : null;
        systemRecord = evnt.Event;
        var linkMeta = systemRecord != null ? SerializationManager.DeserializeMetadata(systemRecord.Metadata) : null;

        var eventType = TypeUtils.ResolveType((linkMeta ?? eventMeta).EventType);
        var deserializer = s_eventDeserializerCache.GetItem(eventType, s_createEventDeserializer);
        return deserializer.ToResolvedEvent(evnt, eventMeta, linkMeta, eventType);
      }
      catch { return evnt.ToResolvedEvent(); }
    }

    internal static IResolvedEvent2 ToResolvedEvent2(this ClientMessage.ResolvedIndexedEvent evnt, EventReadStatus readStatus)
    {
      return readStatus == EventReadStatus.Success
            ? evnt.ToResolvedEvent2()
            : null;
    }

    #endregion

    #region == ToResolvedEvent ==

    internal static ClientAPI.ResolvedEvent<object> ToResolvedEvent(this ClientMessage.ResolvedEvent evnt)
    {
      return new ClientAPI.ResolvedEvent<object>(
                 evnt.Event?.ToRecordedEvent(),
                 evnt.Link?.ToRecordedEvent(),
                 new Position(evnt.CommitPosition, evnt.PreparePosition));
    }

    internal static ClientAPI.ResolvedEvent<object> ToResolvedEvent(this ClientMessage.ResolvedIndexedEvent evnt)
    {
      return new ClientAPI.ResolvedEvent<object>(evnt.Event?.ToRecordedEvent(), evnt.Link?.ToRecordedEvent(), null);
    }

    internal static ClientAPI.ResolvedEvent<object>? ToResolvedEvent(this ClientMessage.ResolvedIndexedEvent evnt, EventReadStatus readStatus)
    {
      return readStatus == EventReadStatus.Success
            ? new ClientAPI.ResolvedEvent<object>(evnt.Event?.ToRecordedEvent(), evnt.Link?.ToRecordedEvent(), null)
            : default(ClientAPI.ResolvedEvent<object>?);
    }

    #endregion

    #region == ToResolvedEvent<T> ==

    internal static ClientAPI.ResolvedEvent<T> ToResolvedEvent<T>(this ClientMessage.ResolvedEvent evnt) where T : class
    {
      return new ClientAPI.ResolvedEvent<T>(
                 evnt.Event?.ToRecordedEvent<T>(),
                 evnt.Link?.ToRecordedEvent<T>(),
                 new Position(evnt.CommitPosition, evnt.PreparePosition));
    }

    internal static ClientAPI.ResolvedEvent<T> ToResolvedEvent<T>(this ClientMessage.ResolvedIndexedEvent evnt) where T : class
    {
      return new ClientAPI.ResolvedEvent<T>(evnt.Event?.ToRecordedEvent<T>(), evnt.Link?.ToRecordedEvent<T>(), null);
    }

    internal static ClientAPI.ResolvedEvent<T>? ToResolvedEvent<T>(this ClientMessage.ResolvedIndexedEvent evnt, EventReadStatus readStatus) where T : class
    {
      return readStatus == EventReadStatus.Success
            ? new ClientAPI.ResolvedEvent<T>(evnt.Event?.ToRecordedEvent<T>(), evnt.Link?.ToRecordedEvent<T>(), null)
            : default(ClientAPI.ResolvedEvent<T>?);
    }

    #endregion

    #region == ToResolvedEvents ==

    internal static ClientAPI.ResolvedEvent<object>[] ToResolvedEvents(this ClientMessage.ResolvedEvent[] events)
    {
      if (events == null || events.Length == 0)
      {
        return EmptyArray<ClientAPI.ResolvedEvent<object>>.Instance;
      }
      else
      {
        var result = new ClientAPI.ResolvedEvent<object>[events.Length];
        for (int i = 0; i < result.Length; ++i)
        {
          result[i] = events[i].ToResolvedEvent();
        }
        return result;
      }
    }

    internal static ClientAPI.ResolvedEvent<object>[] ToResolvedEvents(this ClientMessage.ResolvedIndexedEvent[] events)
    {
      if (events == null || events.Length == 0)
      {
        return EmptyArray<ClientAPI.ResolvedEvent<object>>.Instance;
      }
      else
      {
        var result = new ClientAPI.ResolvedEvent<object>[events.Length];
        for (int i = 0; i < result.Length; ++i)
        {
          result[i] = events[i].ToResolvedEvent();
        }
        return result;
      }
    }

    #endregion

    #region == ToResolvedEvents2 ==

    internal static IResolvedEvent2[] ToResolvedEvents2(this ClientMessage.ResolvedEvent[] events)
    {
      if (events == null || events.Length == 0)
      {
        return EmptyArray<IResolvedEvent2>.Instance;
      }
      else
      {
        var result = new IResolvedEvent2[events.Length];
        for (int i = 0; i < result.Length; ++i)
        {
          result[i] = events[i].ToResolvedEvent2();
        }
        return result;
      }
    }

    internal static IResolvedEvent2[] ToResolvedEvents2(this ClientMessage.ResolvedIndexedEvent[] events)
    {
      if (events == null || events.Length == 0)
      {
        return EmptyArray<IResolvedEvent2>.Instance;
      }
      else
      {
        var result = new IResolvedEvent2[events.Length];
        for (int i = 0; i < result.Length; ++i)
        {
          result[i] = events[i].ToResolvedEvent2();
        }
        return result;
      }
    }

    #endregion

    #region == ToResolvedEvents<T> ==

    internal static ClientAPI.ResolvedEvent<T>[] ToResolvedEvents<T>(this ClientMessage.ResolvedEvent[] events) where T : class
    {
      if (events == null || events.Length == 0)
      {
        return EmptyArray<ClientAPI.ResolvedEvent<T>>.Instance;
      }
      else
      {
        var result = new ClientAPI.ResolvedEvent<T>[events.Length];
        for (int i = 0; i < result.Length; ++i)
        {
          result[i] = events[i].ToResolvedEvent<T>();
        }
        return result;
      }
    }

    internal static ClientAPI.ResolvedEvent<T>[] ToResolvedEvents<T>(this ClientMessage.ResolvedIndexedEvent[] events) where T : class
    {
      if (events == null || events.Length == 0)
      {
        return EmptyArray<ClientAPI.ResolvedEvent<T>>.Instance;
      }
      else
      {
        var result = new ClientAPI.ResolvedEvent<T>[events.Length];
        for (int i = 0; i < result.Length; ++i)
        {
          result[i] = events[i].ToResolvedEvent<T>();
        }
        return result;
      }
    }

    #endregion

    #region == interface IResolvedEventDeserializer ==

    internal interface IResolvedEventDeserializer
    {
      IResolvedEvent2 ToResolvedEvent(ClientMessage.ResolvedEvent evnt, EventMetadata eventMeta, EventMetadata linkMeta, Type eventType);
      IResolvedEvent2 ToResolvedEvent(ClientMessage.ResolvedIndexedEvent evnt, EventMetadata eventMeta, EventMetadata linkMeta, Type eventType);
    }
    internal class ResolvedEventDeserializer<T> : IResolvedEventDeserializer where T : class
    {
      public IResolvedEvent2 ToResolvedEvent(ClientMessage.ResolvedEvent evnt, EventMetadata eventMeta, EventMetadata linkMeta, Type eventType)
      {
        return new ClientAPI.ResolvedEvent<T>(
                   evnt.Event?.ToRecordedEvent<T>(eventMeta, eventType),
                   evnt.Link?.ToRecordedEvent<T>(linkMeta, eventType),
                   new Position(evnt.CommitPosition, evnt.PreparePosition));
      }

      public IResolvedEvent2 ToResolvedEvent(ClientMessage.ResolvedIndexedEvent evnt, EventMetadata eventMeta, EventMetadata linkMeta, Type eventType)
      {
        return new ClientAPI.ResolvedEvent<T>(evnt.Event?.ToRecordedEvent<T>(eventMeta, eventType), 
                                              evnt.Link?.ToRecordedEvent<T>(linkMeta, eventType), null);
      }
    }

    #endregion
  }
}
