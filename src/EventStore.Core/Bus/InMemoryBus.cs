﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using EventStore.Common.Utils;
using EventStore.Core.Messages;
using EventStore.Core.Messaging;

namespace EventStore.Core.Bus
{
    /// <summary>
    /// Synchronously dispatches messages to zero or more subscribers.
    /// Subscribers are responsible for handling exceptions
    /// </summary>
    public class InMemoryBusUnoptimized : IBus, ISubscriber, IPublisher, IHandle<Message>
    {
        public static InMemoryBusUnoptimized CreateTest()
        {
            return new InMemoryBusUnoptimized();
        }

        public static readonly TimeSpan DefaultSlowMessageThreshold = TimeSpan.FromMilliseconds(48);

        private static readonly ILogger Log = TraceLogger.GetLogger<InMemoryBus>();

        public string Name { get; private set; }

        private readonly Dictionary<Type, List<IMessageHandler>> _typeHash;

        private readonly bool _watchSlowMsg;
        private readonly TimeSpan _slowMsgThreshold;

        private InMemoryBusUnoptimized()
          : this("Test")
        {
        }

        public InMemoryBusUnoptimized(string name, bool watchSlowMsg = true, TimeSpan? slowMsgThreshold = null)
        {
            _typeHash = new Dictionary<Type, List<IMessageHandler>>();

            Name = name;
            _watchSlowMsg = watchSlowMsg;
            _slowMsgThreshold = slowMsgThreshold ?? DefaultSlowMessageThreshold;
        }

        public void Subscribe<T>(IHandle<T> handler) where T : Message
        {
            Ensure.NotNull(handler, nameof(handler));

            if (!_typeHash.TryGetValue(typeof(T), out List<IMessageHandler> handlers))
            {
                handlers = new List<IMessageHandler>();
                _typeHash.Add(typeof(T), handlers);
            }
            if (!handlers.Any(x => x.IsSame<T>(handler)))
            {
                handlers.Add(new MessageHandler<T>(handler, handler.GetType().Name));
            }
        }

        public void Unsubscribe<T>(IHandle<T> handler) where T : Message
        {
            Ensure.NotNull(handler, nameof(handler));

            if (_typeHash.TryGetValue(typeof(T), out List<IMessageHandler> handlers))
            {
                var messageHandler = handlers.FirstOrDefault(x => x.IsSame<T>(handler));
                if (messageHandler != null) { handlers.Remove(messageHandler); }
            }
        }

        public void Handle(Message message)
        {
            Publish(message);
        }

        public void Publish(Message message)
        {
            //Ensure.NotNull(message,"message");
            DispatchByType(message);
        }

        private void DispatchByType(Message message)
        {
            var type = message.GetType();
            PublishByType(message, type);
            do
            {
                type = type.BaseType;
                PublishByType(message, type);
            } while (type != typeof(Message));
        }

        private void PublishByType(Message message, Type type)
        {
            if (_typeHash.TryGetValue(type, out List<IMessageHandler> handlers))
            {
                var traceEnabled = Log.IsTraceLevelEnabled();
                for (int i = 0, n = handlers.Count; i < n; ++i)
                {
                    var handler = handlers[i];
                    if (_watchSlowMsg)
                    {
                        var start = DateTime.UtcNow;

                        handler.TryHandle(message);

                        if (traceEnabled)
                        {
                            var elapsed = DateTime.UtcNow - start;
                            if (elapsed > _slowMsgThreshold)
                            {
                                Log.LogTrace("SLOW BUS MSG [{0}]: {1} - {2}ms. Handler: {3}.", Name, message.GetType().Name, (int)elapsed.TotalMilliseconds, handler.HandlerName);
                            }
                        }
                    }
                    else
                    {
                        handler.TryHandle(message);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Synchronously dispatches messages to zero or more subscribers.
    /// Subscribers are responsible for handling exceptions
    /// </summary>
    public class InMemoryBus2 : IBus, ISubscriber, IPublisher, IHandle<Message>
    {
        public static InMemoryBus2 CreateTest()
        {
            return new InMemoryBus2();
        }

        public static readonly TimeSpan DefaultSlowMessageThreshold = TimeSpan.FromMilliseconds(48);

        private static readonly ILogger Log = TraceLogger.GetLogger<InMemoryBus2>();

        public string Name { get; private set; }

        private readonly Dictionary<Type, List<IMessageHandler>> _typeHash;

        private readonly bool _watchSlowMsg;
        private readonly TimeSpan _slowMsgThreshold;

        private InMemoryBus2()
          : this("Test")
        {
        }

        public InMemoryBus2(string name, bool watchSlowMsg = true, TimeSpan? slowMsgThreshold = null)
        {
            _typeHash = new Dictionary<Type, List<IMessageHandler>>();

            Name = name;
            _watchSlowMsg = watchSlowMsg;
            _slowMsgThreshold = slowMsgThreshold ?? DefaultSlowMessageThreshold;
        }

        public void Subscribe<T>(IHandle<T> handler) where T : Message
        {
            Ensure.NotNull(handler, nameof(handler));

            if (!MessageHierarchy.Descendants.TryGetValue(typeof(T), out List<Type> descendants))
            {
                throw new Exception($"No descendants for message of type '{typeof(T).Name}'.");
            }

            foreach (var descendant in descendants)
            {
                if (!_typeHash.TryGetValue(descendant, out List<IMessageHandler> handlers))
                {
                    handlers = new List<IMessageHandler>();
                    _typeHash.Add(descendant, handlers);
                }
                if (!handlers.Any(x => x.IsSame<T>(handler)))
                {
                    handlers.Add(new MessageHandler<T>(handler, handler.GetType().Name));
                }
            }

        }

        public void Unsubscribe<T>(IHandle<T> handler)
          where T : Message
        {
            Ensure.NotNull(handler, nameof(handler));

            if (!MessageHierarchy.Descendants.TryGetValue(typeof(T), out List<Type> descendants))
            {
                throw new Exception($"No descendants for message of type '{typeof(T).Name}'.");
            }

            foreach (var descendant in descendants)
            {
                if (_typeHash.TryGetValue(descendant, out List<IMessageHandler> handlers))
                {
                    var messageHandler = handlers.FirstOrDefault(x => x.IsSame<T>(handler));
                    if (messageHandler != null)
                    {
                        handlers.Remove(messageHandler);
                    }
                }
            }
        }

        public void Handle(Message message)
        {
            Publish(message);
        }

        public void Publish(Message message)
        {
            Ensure.NotNull(message, nameof(message));
            PublishByType(message, message.GetType());
        }

        private void PublishByType(Message message, Type type)
        {
            if (!_typeHash.TryGetValue(type, out List<IMessageHandler> handlers)) { return; }

            var traceEnabled = Log.IsTraceLevelEnabled();
            for (int i = 0, n = handlers.Count; i < n; ++i)
            {
                var handler = handlers[i];
                if (_watchSlowMsg)
                {
                    var start = DateTime.UtcNow;

                    handler.TryHandle(message);

                    if (traceEnabled)
                    {
                        var elapsed = DateTime.UtcNow - start;
                        if (elapsed > _slowMsgThreshold)
                        {
                            Log.LogTrace("SLOW BUS MSG [{0}]: {1} - {2}ms. Handler: {3}.",
                                      Name, message.GetType().Name, (int)elapsed.TotalMilliseconds, handler.HandlerName);
                        }
                    }
                }
                else
                {
                    handler.TryHandle(message);
                }
            }
        }
    }

    /// <summary>Synchronously dispatches messages to zero or more subscribers.
    /// Subscribers are responsible for handling exceptions</summary>
    public class InMemoryBus : IBus, ISubscriber, IPublisher, IHandle<Message>
    {
        public static InMemoryBus CreateTest()
        {
            return new InMemoryBus();
        }

        public static readonly TimeSpan DefaultSlowMessageThreshold = TimeSpan.FromMilliseconds(48);
        private static readonly ILogger Log = TraceLogger.GetLogger<InMemoryBus>();

        public string Name { get; private set; }

        private readonly List<IMessageHandler>[] _handlers;

        private readonly bool _watchSlowMsg;
        private readonly TimeSpan _slowMsgThreshold;

        private InMemoryBus()
          : this("Test")
        {
        }

        public InMemoryBus(string name, bool watchSlowMsg = true, TimeSpan? slowMsgThreshold = null)
        {
            Name = name;
            _watchSlowMsg = watchSlowMsg;
            _slowMsgThreshold = slowMsgThreshold ?? DefaultSlowMessageThreshold;

            _handlers = new List<IMessageHandler>[MessageHierarchy.MaxMsgTypeId + 1];
            for (int i = 0; i < _handlers.Length; ++i)
            {
                _handlers[i] = new List<IMessageHandler>();
            }
        }

        public void Subscribe<T>(IHandle<T> handler) where T : Message
        {
            Ensure.NotNull(handler, nameof(handler));

            int[] descendants = MessageHierarchy.DescendantsByType[typeof(T)];
            for (int i = 0; i < descendants.Length; ++i)
            {
                var handlers = _handlers[descendants[i]];
                if (!handlers.Any(x => x.IsSame<T>(handler)))
                {
                    handlers.Add(new MessageHandler<T>(handler, handler.GetType().Name));
                }
            }
        }

        public void Unsubscribe<T>(IHandle<T> handler) where T : Message
        {
            Ensure.NotNull(handler, nameof(handler));

            int[] descendants = MessageHierarchy.DescendantsByType[typeof(T)];
            for (int i = 0; i < descendants.Length; ++i)
            {
                var handlers = _handlers[descendants[i]];
                var messageHandler = handlers.FirstOrDefault(x => x.IsSame<T>(handler));
                if (messageHandler != null)
                {
                    handlers.Remove(messageHandler);
                }
            }
        }

        public void Handle(Message message)
        {
            Publish(message);
        }

        public void Publish(Message message)
        {
            //if (message == null) throw new ArgumentNullException("message");

            var traceEnabled = Log.IsTraceLevelEnabled();
            var handlers = _handlers[message.MsgTypeId];
            for (int i = 0, n = handlers.Count; i < n; ++i)
            {
                var handler = handlers[i];
                if (_watchSlowMsg)
                {
                    var start = DateTime.UtcNow;

                    handler.TryHandle(message);

                    var elapsed = DateTime.UtcNow - start;
                    if (elapsed > _slowMsgThreshold)
                    {
                        if (traceEnabled)
                        {
                            Log.LogTrace("SLOW BUS MSG [{0}]: {1} - {2}ms. Handler: {3}.",
                                    Name, message.GetType().Name, (int)elapsed.TotalMilliseconds, handler.HandlerName);
                        }
                        if (elapsed > QueuedHandler.VerySlowMsgThreshold && !(message is SystemMessage.SystemInit))
                        {
                            Log.LogError("---!!! VERY SLOW BUS MSG [{0}]: {1} - {2}ms. Handler: {3}.",
                                      Name, message.GetType().Name, (int)elapsed.TotalMilliseconds, handler.HandlerName);
                        }
                    }
                }
                else
                {
                    handler.TryHandle(message);
                }
            }
        }
    }
}