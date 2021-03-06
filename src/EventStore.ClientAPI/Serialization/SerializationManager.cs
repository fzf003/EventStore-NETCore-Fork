﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CuteAnt;
using CuteAnt.Collections;
using CuteAnt.Extensions.Serialization;
using CuteAnt.Reflection;
using EventStore.ClientAPI.Exceptions;
using EventStore.ClientAPI.Internal;
using MessagePack;
using MessagePack.Formatters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Utf8Json;

namespace EventStore.ClientAPI.Serialization
{
  /// <summary>SerializationManager to oversee the EventStore serializer system.</summary>
  public static class SerializationManager
  {
    #region @@ Fields @@

    private static ILogger s_logger = TraceLogger.GetLogger(typeof(SerializationManager));
    private static IList<IExternalSerializer> _externalSerializers;
    private static readonly CachedReadConcurrentDictionary<Type, IExternalSerializer> _typeToExternalSerializerDictionary;

    private static readonly CachedReadConcurrentDictionary<Type, StreamAttribute> _typeToStreamProviderDictionary;

    private static readonly CachedReadConcurrentDictionary<Type, SerializingTokenAttribute> _typeToSerializationTokenDictionary;

    private static readonly JsonSerializerSettings _metadataSettings;
    private static readonly IJsonMessageFormatter _jsonFormatter;

    private static IMessageFormatter _jsonSerializer;
    private static IMessageFormatter _utf8JsonSerializer;
    private static IMessageFormatter _messagePackSerializer;
    private static IMessageFormatter _lz4MessagePackSerializer;
    private static IMessageFormatter _protobufSerializer;

    #endregion

    #region @@ Constructor @@

    static SerializationManager()
    {
      // Preserve object references
      MessagePackStandardResolver.Register(
        HyperionResolver.Instance, HyperionExceptionResolver.Instance, HyperionExpressionResolver.Instance);

      _externalSerializers = new List<IExternalSerializer>();
      _typeToExternalSerializerDictionary = new CachedReadConcurrentDictionary<Type, IExternalSerializer>();
      _typeToStreamProviderDictionary = new CachedReadConcurrentDictionary<Type, StreamAttribute>();
      _typeToSerializationTokenDictionary = new CachedReadConcurrentDictionary<Type, SerializingTokenAttribute>();

      _metadataSettings = JsonConvertX.CreateSerializerSettings(Formatting.Indented);
      _metadataSettings.Converters.Add(JsonConvertX.DefaultStringEnumCamelCaseConverter);
      _jsonFormatter = new JsonMessageFormatter()
      {
        DefaultSerializerSettings = _metadataSettings,
        DefaultDeserializerSettings = _metadataSettings
      };

      _jsonSerializer = JsonMessageFormatter.DefaultInstance;
      _utf8JsonSerializer = Utf8JsonMessageFormatter.DefaultInstance;
      _messagePackSerializer = MessagePackMessageFormatter.DefaultInstance;
      _lz4MessagePackSerializer = LZ4MessagePackMessageFormatter.DefaultInstance;
      _protobufSerializer = ProtoBufMessageFormatter.DefaultInstance;
    }

    #endregion

    #region -- Register --

    /// <summary>Json message formatter</summary>
    /// <param name="serializerSettings"></param>
    /// <param name="deserializerSettings"></param>
    public static void Register(JsonSerializerSettings serializerSettings, JsonSerializerSettings deserializerSettings)
    {
      if (null == serializerSettings) { throw new ArgumentNullException(nameof(serializerSettings)); }
      if (null == deserializerSettings) { throw new ArgumentNullException(nameof(deserializerSettings)); }

      _jsonSerializer = new JsonMessageFormatter()
      {
        DefaultSerializerSettings = serializerSettings,
        DefaultDeserializerSettings = deserializerSettings
      };
    }

    /// <summary>Utf8Json message formatter</summary>
    public static void Register(IJsonFormatter[] formatters, IJsonFormatterResolver[] resolvers)
    {
      Utf8JsonStandardResolver.Register(formatters, resolvers);
    }

    /// <summary>MessagePack message formatter</summary>
    public static void Register(IMessagePackFormatter[] formatters, IFormatterResolver[] resolvers,
      IMessagePackFormatter[] typelessFormatters, IFormatterResolver[] typelessResolvers)
    {
      MessagePackStandardResolver.Register(formatters, resolvers);
    }

    #endregion

    #region -- RegisterSerializationProvider --

    public static void RegisterSerializationProvider(IExternalSerializer serializer, int? insertIndex = null)
    {
      if (null == serializer) { throw new ArgumentNullException(nameof(serializer)); }

      if (insertIndex.HasValue)
      {
        // 插入失败，也需要添加
        try
        {
          _externalSerializers.Insert(insertIndex.Value, serializer);
          return;
        }
        catch { }
      }
      _externalSerializers.Add(serializer);
    }

    /// <summary>Loads the external srializers and places them into a hash set</summary>
    /// <param name="providerTypes">The list of types that implement <see cref="IExternalSerializer"/></param>
    public static void RegisterSerializationProviders(List<TypeInfo> providerTypes)
    {
      if (providerTypes == null) { return; }

      _externalSerializers.Clear();
      _typeToExternalSerializerDictionary.Clear();
      providerTypes.ForEach(typeInfo =>
      {
        try
        {
          var serializer = ActivatorUtils.FastCreateInstance<IExternalSerializer>(typeInfo.AsType());
          _externalSerializers.Add(serializer);
        }
        catch (Exception exception)
        {
          s_logger.LogError(exception, $"Failed to create instance of type: {typeInfo.FullName}");
        }
      });
    }

    #endregion

    #region ** TryLookupExternalSerializer **

    private static bool TryLookupExternalSerializer(Type t, out IExternalSerializer serializer)
    {
      // essentially a no-op if there are no external serializers registered
      if (_externalSerializers.Count == 0)
      {
        serializer = null;
        return false;
      }

      // the associated serializer will be null if there are no external serializers that handle this type
      if (_typeToExternalSerializerDictionary.TryGetValue(t, out serializer))
      {
        return serializer != null;
      }

      serializer = _externalSerializers.FirstOrDefault(s => s.IsSupportedType(t));

      _typeToExternalSerializerDictionary.TryAdd(t, serializer);

      return serializer != null;
    }

    #endregion

    #region -- RegisterStreamProvider --

    public static void RegisterStreamProvider(Type expectedType, string stream, string eventType = null, string expectedVersion = null)
    {
      if (null == expectedType) { throw new ArgumentNullException(nameof(expectedType)); }
      if (string.IsNullOrEmpty(stream)) { throw new ArgumentNullException(nameof(stream)); }

      if (string.IsNullOrWhiteSpace(eventType)) { eventType = RuntimeTypeNameFormatter.Serialize(expectedType); }
      _typeToStreamProviderDictionary.TryAdd(expectedType, new StreamAttribute(stream, eventType, expectedVersion));
    }

    #endregion

    #region ** TryLookupStreamProvider **

    private static bool TryLookupStreamProvider(Type expectedType, out StreamAttribute streamAttr)
    {
      if (_typeToStreamProviderDictionary.TryGetValue(expectedType, out streamAttr))
      {
        return streamAttr != null;
      }

      streamAttr = expectedType.GetCustomAttributeX<StreamAttribute>();

      _typeToStreamProviderDictionary.TryAdd(expectedType, streamAttr);

      return streamAttr != null;
    }

    #endregion

    #region == GetStreamProvider ==

    internal static StreamAttribute GetStreamProvider(Type actualType, Type expectedType = null)
    {
      if (expectedType != null && TryLookupStreamProvider(expectedType, out StreamAttribute streamAttr))
      {
        return streamAttr;
      }
      else if (TryLookupStreamProvider(actualType, out streamAttr))
      {
        return streamAttr;
      }
      return null;
    }

    #endregion

    #region -- RegisterSerializingToken --

    public static void RegisterSerializingToken(Type expectedType, SerializingToken token)
    {
      if (null == expectedType) { throw new ArgumentNullException(nameof(expectedType)); }

      _typeToSerializationTokenDictionary.TryAdd(expectedType, new SerializingTokenAttribute(token));
    }

    #endregion

    #region ** TryLookupSerializingToken **

    private static bool TryLookupSerializingToken(Type expectedType, out SerializingTokenAttribute tokenAttr)
    {
      if (_typeToSerializationTokenDictionary.TryGetValue(expectedType, out tokenAttr))
      {
        return tokenAttr != null;
      }

      tokenAttr = expectedType.GetCustomAttributeX<SerializingTokenAttribute>();

      _typeToSerializationTokenDictionary.TryAdd(expectedType, tokenAttr);

      return tokenAttr != null;
    }

    #endregion

    #region ** GetSerializingToken **

    internal static SerializingToken GetSerializingToken(Type actualType, Type expectedType = null)
    {
      var token = SerializingToken.Json;
      if (expectedType != null && TryLookupSerializingToken(expectedType, out SerializingTokenAttribute tokenAttr))
      {
        token = tokenAttr.Token;
      }
      else if (TryLookupSerializingToken(actualType, out tokenAttr))
      {
        token = tokenAttr.Token;
      }
      else
      {
        var msgPackContract = actualType.GetCustomAttributeX<MessagePackObjectAttribute>();
        if (msgPackContract != null) { return SerializingToken.MessagePack; }
        var utf8JsonContract = actualType.GetCustomAttributeX<JsonFormatterAttribute>();
        if (utf8JsonContract != null) { return SerializingToken.Utf8Json; }
      }
      return token;
    }

    #endregion

    #region -- GetStreamId --

    internal static string GetStreamId(Type actualType, Type expectedType = null)
    {
      var streamAttr = GetStreamProvider(actualType, expectedType);
      return streamAttr != null ? streamAttr.StreamId : RuntimeTypeNameFormatter.Serialize(expectedType ?? actualType);
    }

    #endregion

    #region -- SerializeEvent --

    public static EventData SerializeEvent(object @event, Dictionary<string, object> eventContext = null, Type expectedType = null)
    {
      if (null == @event) { throw new ArgumentNullException(nameof(@event)); }

      return SerializeEvent(@event.GetType(), @event, eventContext, expectedType);
    }
    public static EventData SerializeEvent(Type actualType, object @event, Dictionary<string, object> eventContext = null, Type expectedType = null)
    {
      if (null == @event) { throw new ArgumentNullException(nameof(@event)); }
      if (null == actualType) { throw new ArgumentNullException(nameof(actualType)); }

      var streamAttr = GetStreamProvider(actualType, expectedType);
      return SerializeEvent(streamAttr?.EventType, actualType, @event, eventContext, expectedType);
    }


    internal static EventData SerializeEvent(StreamAttribute streamAttr, object @event, Dictionary<string, object> eventContext = null, Type expectedType = null)
    {
      //if (null == streamAttr) { throw new ArgumentNullException(nameof(streamAttr)); }

      return SerializeEvent(streamAttr?.EventType, @event, eventContext, expectedType);
    }
    internal static EventData SerializeEvent(StreamAttribute streamAttr, Type actualType, object @event, Dictionary<string, object> eventContext = null, Type expectedType = null)
    {
      //if (null == streamAttr) { throw new ArgumentNullException(nameof(streamAttr)); }

      return SerializeEvent(streamAttr?.EventType, actualType, @event, eventContext, expectedType);
    }


    public static EventData SerializeEvent(string eventType, object @event, Dictionary<string, object> eventContext = null, Type expectedType = null)
    {
      if (null == @event) { throw new ArgumentNullException(nameof(@event)); }

      return SerializeEvent(eventType, @event.GetType(), @event, eventContext, expectedType);
    }
    public static EventData SerializeEvent(string eventType, Type actualType, object @event, Dictionary<string, object> eventContext = null, Type expectedType = null)
    {
      if (null == actualType) { throw new ArgumentNullException(nameof(actualType)); }
      if (null == @event) { throw new ArgumentNullException(nameof(@event)); }

      var token = GetSerializingToken(actualType, expectedType);
      return SerializeEvent(token, eventType, actualType, @event, eventContext, expectedType);
    }

    internal static EventData SerializeEvent(SerializingToken token, string eventType, Type actualType, object @event, Dictionary<string, object> eventContext, Type expectedType)
    {
      if (string.IsNullOrWhiteSpace(eventType)) { eventType = RuntimeTypeNameFormatter.Serialize(expectedType ?? actualType); }
      byte[] data;
      switch (token)
      {
        case SerializingToken.Utf8Json:
          data = _utf8JsonSerializer.SerializeObject(@event);
          break;
        case SerializingToken.MessagePack:
          data = _messagePackSerializer.SerializeObject(@event);
          break;
        case SerializingToken.Lz4MessagePack:
          data = _lz4MessagePackSerializer.SerializeObject(@event);
          break;
        case SerializingToken.Protobuf:
          data = _protobufSerializer.SerializeObject(@event);
          break;
        case SerializingToken.External:
          // 此处不考虑 expectedType
          if (TryLookupExternalSerializer(actualType, out IExternalSerializer serializer))
          {
            data = serializer.SerializeObject(@event);
          }
          else
          {
            throw new InvalidOperationException($"Non-serializable exception of type {actualType.AssemblyQualifiedName}");
          }
          break;
        case SerializingToken.Json:
        default:
          data = _jsonSerializer.Serialize(@event);
          break;
      }
      return new EventData(
          Guid.NewGuid(), eventType, SerializingToken.Json == token, data,
          _jsonFormatter.SerializeObject(new EventMetadata
          {
            EventType = RuntimeTypeNameFormatter.Serialize(actualType),
            Token = token,
            Context = eventContext
          }));
    }

    #endregion

    #region -- SerializeEvents --

    public static EventData[] SerializeEvents<TEvent>(IEnumerable<TEvent> events, Dictionary<string, object> eventContext = null, Type expectedType = null)
    {
      if (null == events) { throw new ArgumentNullException(nameof(events)); }

      var actualType = typeof(TEvent);
      if (actualType == TypeConstants.ObjectType)
      {
        return events.Select(_ => SerializeEvent(_, eventContext, expectedType)).ToArray();
      }
      else
      {
        return SerializeEvents(actualType, events, eventContext, expectedType);
      }
    }
    public static EventData[] SerializeEvents<TEvent>(Type actualType, IEnumerable<TEvent> events, Dictionary<string, object> eventContext = null, Type expectedType = null)
    {
      if (null == actualType) { throw new ArgumentNullException(nameof(actualType)); }

      var streamAttr = GetStreamProvider(actualType, expectedType);
      return SerializeEvents(streamAttr?.EventType, actualType, events, eventContext, expectedType);
    }

    internal static EventData[] SerializeEvents<TEvent>(StreamAttribute streamAttr, IEnumerable<TEvent> events, Dictionary<string, object> eventContext = null, Type expectedType = null)
    {
      //if (null == streamAttr) { throw new ArgumentNullException(nameof(streamAttr)); }
      return SerializeEvents(streamAttr?.EventType, events, eventContext, expectedType);
    }
    internal static EventData[] SerializeEvents<TEvent>(StreamAttribute streamAttr, Type actualType, IEnumerable<TEvent> events, Dictionary<string, object> eventContext = null, Type expectedType = null)
    {
      //if (null == streamAttr) { throw new ArgumentNullException(nameof(streamAttr)); }
      return SerializeEvents(streamAttr?.EventType, actualType, events, eventContext, expectedType);
    }

    public static EventData[] SerializeEvents<TEvent>(string eventType, IEnumerable<TEvent> events, Dictionary<string, object> eventContext = null, Type expectedType = null)
    {
      if (null == events) { throw new ArgumentNullException(nameof(events)); }

      var actualType = typeof(TEvent);
      if (actualType == TypeConstants.ObjectType)
      {
        return events.Select(_ => SerializeEvent(eventType, _, eventContext, expectedType)).ToArray();
      }
      else
      {
        return SerializeEvents(eventType, actualType, events, eventContext, expectedType);
      }
    }

    public static EventData[] SerializeEvents<TEvent>(string eventType, Type actualType, IEnumerable<TEvent> events, Dictionary<string, object> eventContext = null, Type expectedType = null)
    {
      if (null == actualType) { throw new ArgumentNullException(nameof(actualType)); }
      if (null == events) { throw new ArgumentNullException(nameof(events)); }

      var token = GetSerializingToken(actualType, expectedType);
      return events.Select(_ => SerializeEvent(token, eventType, actualType, _, eventContext, expectedType)).ToArray();
    }




    public static EventData[] SerializeEvents<TEvent>(IList<TEvent> events, IList<Dictionary<string, object>> eventContexts, Type expectedType = null)
    {
      if (null == events) { throw new ArgumentNullException(nameof(events)); }
      if (null == eventContexts) { throw new ArgumentNullException(nameof(eventContexts)); }
      if (events.Count != eventContexts.Count) { throw new ArgumentOutOfRangeException(nameof(eventContexts)); }

      var actualType = typeof(TEvent);
      if (actualType == TypeConstants.ObjectType)
      {
        var list = new EventData[events.Count];
        for (var idx = 0; idx < events.Count; idx++)
        {
          list[idx] = SerializeEvent(events[idx], eventContexts[idx], expectedType);
        }
        return list;
      }
      else
      {
        return SerializeEvents(actualType, events, eventContexts, expectedType);
      }
    }
    public static EventData[] SerializeEvents<TEvent>(Type actualType, IList<TEvent> events, IList<Dictionary<string, object>> eventContexts, Type expectedType = null)
    {
      if (null == actualType) { throw new ArgumentNullException(nameof(actualType)); }

      var streamAttr = GetStreamProvider(actualType, expectedType);
      return SerializeEvents(streamAttr?.EventType, actualType, events, eventContexts, expectedType);
    }

    internal static EventData[] SerializeEvents<TEvent>(StreamAttribute streamAttr, IList<TEvent> events, IList<Dictionary<string, object>> eventContexts, Type expectedType = null)
    {
      if (null == streamAttr) { throw new ArgumentNullException(nameof(streamAttr)); }
      return SerializeEvents(streamAttr.EventType, events, eventContexts, expectedType);
    }
    internal static EventData[] SerializeEvents<TEvent>(StreamAttribute streamAttr, Type actualType, IList<TEvent> events, IList<Dictionary<string, object>> eventContexts, Type expectedType = null)
    {
      if (null == streamAttr) { throw new ArgumentNullException(nameof(streamAttr)); }
      return SerializeEvents(streamAttr.EventType, actualType, events, eventContexts, expectedType);
    }

    public static EventData[] SerializeEvents<TEvent>(string eventType, IList<TEvent> events, IList<Dictionary<string, object>> eventContexts, Type expectedType = null)
    {
      if (null == events) { throw new ArgumentNullException(nameof(events)); }
      if (null == eventContexts) { throw new ArgumentNullException(nameof(eventContexts)); }
      if (events.Count != eventContexts.Count) { throw new ArgumentOutOfRangeException(nameof(eventContexts)); }

      var actualType = typeof(TEvent);
      if (actualType == TypeConstants.ObjectType)
      {
        var list = new EventData[events.Count];
        for (var idx = 0; idx < events.Count; idx++)
        {
          list[idx] = SerializeEvent(eventType, events[idx], eventContexts[idx], expectedType);
        }
        return list;
      }
      else
      {
        return SerializeEvents(eventType, actualType, events, eventContexts, expectedType);
      }
    }

    public static EventData[] SerializeEvents<TEvent>(string eventType, Type actualType, IList<TEvent> events, IList<Dictionary<string, object>> eventContexts, Type expectedType = null)
    {
      if (null == actualType) { throw new ArgumentNullException(nameof(actualType)); }
      if (null == events) { throw new ArgumentNullException(nameof(events)); }
      if (null == eventContexts) { throw new ArgumentNullException(nameof(eventContexts)); }
      if (events.Count != eventContexts.Count) { throw new ArgumentOutOfRangeException(nameof(eventContexts)); }

      var token = GetSerializingToken(actualType, expectedType);
      var list = new EventData[events.Count];
      for (var idx = 0; idx < events.Count; idx++)
      {
        list[idx] = SerializeEvent(token, eventType, actualType, events[idx], eventContexts[idx], expectedType);
      }

      return list;
    }

    #endregion

    #region -- DeserializeEvent --

    public static EventMetadata DeserializeMetadata(byte[] metadata)
    {
      const string _metadataEmpty = "The meta-data of EventRecord is not available.";
      if (null == metadata || metadata.Length == 0)
      {
        throw new EventMetadataDeserializationException(_metadataEmpty);
      }
      var meta = _jsonFormatter.Deserialize<EventMetadata>(metadata);
      if (null == meta)
      {
        throw new EventMetadataDeserializationException(_metadataEmpty);
      }
      return meta;
    }

    public static IFullEvent DeserializeEvent(EventData eventData)
    {
      return DeserializeEvent(eventData.Metadata, eventData.Data);
    }
    public static IFullEvent<T> DeserializeEvent<T>(EventData eventData) where T : class
    {
      return DeserializeEvent<T>(eventData.Metadata, eventData.Data);
    }

    public static IFullEvent DeserializeEvent(byte[] metadata, byte[] data)
    {
      var meta = DeserializeMetadata(metadata);
      DeserializeEvent(meta, null, data, out IEventDescriptor eventDescriptor, out object obj);
      return new DefaultFullEvent { Descriptor = eventDescriptor, Value = obj };
    }

    public static IFullEvent<T> DeserializeEvent<T>(byte[] metadata, byte[] data) where T : class
    {
      var meta = DeserializeMetadata(metadata);
      DeserializeEvent(meta, null, data, out IEventDescriptor eventDescriptor, out object obj);
      return new DefaultFullEvent<T> { Descriptor = eventDescriptor, Value = obj as T };
    }


    public static IFullEvent DeserializeEvent(EventMetadata metadata, byte[] data)
    {
      return DeserializeEvent(metadata, null, data);
    }
    public static IFullEvent DeserializeEvent(EventMetadata metadata, Type eventType, byte[] data)
    {
      if (null == metadata) { throw new ArgumentNullException(nameof(metadata)); }

      DeserializeEvent(metadata, eventType, data, out IEventDescriptor eventDescriptor, out object obj);
      return new DefaultFullEvent { Descriptor = eventDescriptor, Value = obj };
    }

    public static IFullEvent<T> DeserializeEvent<T>(EventMetadata metadata, byte[] data) where T : class
    {
      return DeserializeEvent<T>(metadata, null, data);
    }
    public static IFullEvent<T> DeserializeEvent<T>(EventMetadata metadata, Type eventType, byte[] data) where T : class
    {
      if (null == metadata) { throw new ArgumentNullException(nameof(metadata)); }

      DeserializeEvent(metadata, eventType, data, out IEventDescriptor eventDescriptor, out object obj);
      return new DefaultFullEvent<T> { Descriptor = eventDescriptor, Value = obj as T };
    }

    private static void DeserializeEvent(EventMetadata meta, Type eventType, byte[] data, out IEventDescriptor eventDescriptor, out object obj)
    {
      eventDescriptor = meta.Context != null ? new DefaultEventDescriptor(meta.Context) : NullEventDescriptor.Instance;

      if (null == data || data.Length == 0)
      {
        obj = null;
        return;
      }
      try
      {
        var type = eventType ?? TypeUtils.ResolveType(meta.EventType);
        switch (meta.Token)
        {
          case SerializingToken.Utf8Json:
            obj = _utf8JsonSerializer.Deserialize(type, data);
            break;
          case SerializingToken.MessagePack:
            obj = _messagePackSerializer.Deserialize(type, data);
            break;
          case SerializingToken.Lz4MessagePack:
            obj = _lz4MessagePackSerializer.Deserialize(type, data);
            break;
          case SerializingToken.Protobuf:
            obj = _protobufSerializer.Deserialize(type, data);
            break;
          case SerializingToken.External:
            if (TryLookupExternalSerializer(type, out IExternalSerializer serializer))
            {
              obj = serializer.Deserialize(type, data);
            }
            else
            {
              throw new Exception($"Non-serializable exception of type {type.AssemblyQualifiedName}");
            }
            break;
          case SerializingToken.Json:
          default:
            obj = _jsonSerializer.Deserialize(type, data);
            break;
        }
      }
      catch (Exception exc)
      {
        throw new EventDataDeserializationException(exc.Message, exc);
      }
    }

    #endregion
  }
}
