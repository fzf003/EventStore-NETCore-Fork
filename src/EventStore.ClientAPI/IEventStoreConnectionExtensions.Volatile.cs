﻿using System;
using System.Threading.Tasks;
using CuteAnt.AsyncEx;
using EventStore.ClientAPI.SystemData;

namespace EventStore.ClientAPI
{
  partial class IEventStoreConnectionExtensions
  {
    #region -- SubscribeToStream --

    /// <summary>Subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase"/> responsible for raising the event.</param>
    /// <param name="stream">The stream to subscribe to</param>
    /// <param name="resolveLinkTos">Whether to resolve Link events automatically</param>
    /// <param name="eventAppeared">An action invoked when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <param name="verboseLogging">Enables verbose logging on the subscription</param>
    /// <returns>A <see cref="EventStoreSubscription"/> representing the subscription.</returns>
    public static EventStoreSubscription SubscribeToStream(this IEventStoreConnectionBase connection, string stream, bool resolveLinkTos,
      Action<EventStoreSubscription, ResolvedEvent> eventAppeared,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null, bool verboseLogging = false)
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      var subscriptionSettings = new SubscriptionSettings { ResolveLinkTos = resolveLinkTos, VerboseLogging = verboseLogging };
      return AsyncContext.Run(
        async (conn, streamId, settings, eAppeared, subDropped, credentials)
          => await conn.SubscribeToStreamAsync(streamId, settings, eAppeared, subDropped, credentials).ConfigureAwait(false),
        connection, stream, subscriptionSettings, eventAppeared, subscriptionDropped, userCredentials);
    }

    /// <summary>Subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase"/> responsible for raising the event.</param>
    /// <param name="stream">The stream to subscribe to</param>
    /// <param name="resolveLinkTos">Whether to resolve Link events automatically</param>
    /// <param name="eventAppearedAsync">A Task invoked and awaited when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <param name="verboseLogging">Enables verbose logging on the subscription</param>
    /// <returns>A <see cref="EventStoreSubscription"/> representing the subscription.</returns>
    public static EventStoreSubscription SubscribeToStream(this IEventStoreConnectionBase connection, string stream, bool resolveLinkTos,
      Func<EventStoreSubscription, ResolvedEvent, Task> eventAppearedAsync,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null, bool verboseLogging = false)
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      var subscriptionSettings = new SubscriptionSettings { ResolveLinkTos = resolveLinkTos, VerboseLogging = verboseLogging };
      return AsyncContext.Run(
        async (conn, streamId, settings, eAppeared, subDropped, credentials)
          => await conn.SubscribeToStreamAsync(streamId, settings, eAppeared, subDropped, credentials).ConfigureAwait(false),
        connection, stream, subscriptionSettings, eventAppearedAsync, subscriptionDropped, userCredentials);
    }

    /// <summary>Subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase"/> responsible for raising the event.</param>
    /// <param name="stream">The stream to subscribe to</param>
    /// <param name="subscriptionSettings">The <see cref="SubscriptionSettings"/> for the subscription</param>
    /// <param name="eventAppeared">An action invoked when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <returns>A <see cref="EventStoreSubscription"/> representing the subscription.</returns>
    public static EventStoreSubscription SubscribeToStream(this IEventStoreConnectionBase connection,
      string stream, SubscriptionSettings subscriptionSettings,
      Action<EventStoreSubscription, ResolvedEvent> eventAppeared,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null)
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      return AsyncContext.Run(
        async (conn, streamId, settings, eAppeared, subDropped, credentials)
          => await conn.SubscribeToStreamAsync(streamId, settings, eAppeared, subDropped, credentials).ConfigureAwait(false),
        connection, stream, subscriptionSettings, eventAppeared, subscriptionDropped, userCredentials);
    }

    /// <summary>Subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase"/> responsible for raising the event.</param>
    /// <param name="stream">The stream to subscribe to</param>
    /// <param name="subscriptionSettings">The <see cref="SubscriptionSettings"/> for the subscription</param>
    /// <param name="eventAppearedAsync">A Task invoked and awaited when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <returns>A <see cref="EventStoreSubscription"/> representing the subscription.</returns>
    public static EventStoreSubscription SubscribeToStream(this IEventStoreConnectionBase connection,
      string stream, SubscriptionSettings subscriptionSettings,
      Func<EventStoreSubscription, ResolvedEvent, Task> eventAppearedAsync,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null)
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      return AsyncContext.Run(
        async (conn, streamId, settings, eAppeared, subDropped, credentials)
          => await conn.SubscribeToStreamAsync(streamId, settings, eAppeared, subDropped, credentials).ConfigureAwait(false),
        connection, stream, subscriptionSettings, eventAppearedAsync, subscriptionDropped, userCredentials);
    }

    #endregion

    #region -- SubscribeToStreamAsync --

    /// <summary>Asynchronously subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase"/> responsible for raising the event.</param>
    /// <param name="stream">The stream to subscribe to</param>
    /// <param name="resolveLinkTos">Whether to resolve Link events automatically</param>
    /// <param name="eventAppeared">An action invoked when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <returns>A <see cref="Task&lt;EventStoreSubscription&gt;"/> representing the subscription.</returns>
    public static Task<EventStoreSubscription> SubscribeToStreamAsync(this IEventStoreConnectionBase connection, string stream, bool resolveLinkTos,
      Action<EventStoreSubscription, ResolvedEvent> eventAppeared,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null)
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      var settings = new SubscriptionSettings { ResolveLinkTos = resolveLinkTos };
      return connection.SubscribeToStreamAsync(stream, settings, eventAppeared, subscriptionDropped, userCredentials);
    }

    /// <summary>Asynchronously subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase"/> responsible for raising the event.</param>
    /// <param name="stream">The stream to subscribe to</param>
    /// <param name="resolveLinkTos">Whether to resolve Link events automatically</param>
    /// <param name="eventAppearedAsync">A Task invoked and awaited when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <returns>A <see cref="Task&lt;EventStoreSubscription&gt;"/> representing the subscription.</returns>
    public static Task<EventStoreSubscription> SubscribeToStreamAsync(this IEventStoreConnectionBase connection, string stream, bool resolveLinkTos,
      Func<EventStoreSubscription, ResolvedEvent, Task> eventAppearedAsync,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null)
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      var settings = new SubscriptionSettings { ResolveLinkTos = resolveLinkTos };
      return connection.SubscribeToStreamAsync(stream, settings, eventAppearedAsync, subscriptionDropped, userCredentials);
    }

    #endregion

    #region -- SubscribeToAll --

    /// <summary>Subscribes to all events in the Event Store. New
    /// events written to the stream while the subscription is active
    /// will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnection"/> responsible for raising the event.</param>
    /// <param name="resolveLinkTos">Whether to resolve Link events automatically</param>
    /// <param name="eventAppeared">An action invoked when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <param name="verboseLogging">Enables verbose logging on the subscription</param>
    /// <returns>A <see cref="EventStoreSubscription"/> representing the subscription.</returns>
    public static EventStoreSubscription SubscribeToAll(this IEventStoreConnection connection, bool resolveLinkTos,
      Action<EventStoreSubscription, ResolvedEvent> eventAppeared,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null, bool verboseLogging = false)
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      var settings = new SubscriptionSettings { ResolveLinkTos = resolveLinkTos, VerboseLogging = verboseLogging };
      return connection.SubscribeToAllAsync(settings, eventAppeared, subscriptionDropped, userCredentials)
                       .ConfigureAwait(false).GetAwaiter().GetResult();
    }

    /// <summary>Subscribes to all events in the Event Store. New
    /// events written to the stream while the subscription is active
    /// will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnection"/> responsible for raising the event.</param>
    /// <param name="resolveLinkTos">Whether to resolve Link events automatically</param>
    /// <param name="eventAppearedAsync">A Task invoked and awaited when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <param name="verboseLogging">Enables verbose logging on the subscription</param>
    /// <returns>A <see cref="EventStoreSubscription"/> representing the subscription.</returns>
    public static EventStoreSubscription SubscribeToAll(this IEventStoreConnection connection, bool resolveLinkTos,
      Func<EventStoreSubscription, ResolvedEvent, Task> eventAppearedAsync,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null, bool verboseLogging = false)
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      var settings = new SubscriptionSettings { ResolveLinkTos = resolveLinkTos, VerboseLogging = verboseLogging };
      return connection.SubscribeToAllAsync(settings, eventAppearedAsync, subscriptionDropped, userCredentials)
                       .ConfigureAwait(false).GetAwaiter().GetResult();
    }

    /// <summary>Subscribes to all events in the Event Store. New
    /// events written to the stream while the subscription is active
    /// will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnection"/> responsible for raising the event.</param>
    /// <param name="settings">The <see cref="SubscriptionSettings"/> for the subscription</param>
    /// <param name="eventAppeared">An action invoked when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <returns>A <see cref="EventStoreSubscription"/> representing the subscription.</returns>
    public static EventStoreSubscription SubscribeToAll(this IEventStoreConnection connection, SubscriptionSettings settings,
      Action<EventStoreSubscription, ResolvedEvent> eventAppeared,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null)
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      return connection.SubscribeToAllAsync(settings, eventAppeared, subscriptionDropped, userCredentials)
                       .ConfigureAwait(false).GetAwaiter().GetResult();
    }

    /// <summary>Subscribes to all events in the Event Store. New
    /// events written to the stream while the subscription is active
    /// will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnection"/> responsible for raising the event.</param>
    /// <param name="settings">The <see cref="SubscriptionSettings"/> for the subscription</param>
    /// <param name="eventAppearedAsync">A Task invoked and awaited when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <returns>A <see cref="EventStoreSubscription"/> representing the subscription.</returns>
    public static EventStoreSubscription SubscribeToAll(this IEventStoreConnection connection, SubscriptionSettings settings,
      Func<EventStoreSubscription, ResolvedEvent, Task> eventAppearedAsync,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null)
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      return connection.SubscribeToAllAsync(settings, eventAppearedAsync, subscriptionDropped, userCredentials)
                       .ConfigureAwait(false).GetAwaiter().GetResult();
    }

    #endregion

    #region -- SubscribeToAllAsync --

    /// <summary>Asynchronously subscribes to all events in the Event Store. New
    /// events written to the stream while the subscription is active
    /// will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnection"/> responsible for raising the event.</param>
    /// <param name="resolveLinkTos">Whether to resolve Link events automatically</param>
    /// <param name="eventAppeared">An action invoked when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <returns>A <see cref="Task&lt;EventStoreSubscription&gt;"/> representing the subscription.</returns>
    public static Task<EventStoreSubscription> SubscribeToAllAsync(this IEventStoreConnection connection, bool resolveLinkTos,
      Action<EventStoreSubscription, ResolvedEvent> eventAppeared,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null)
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      var settings = new SubscriptionSettings { ResolveLinkTos = resolveLinkTos };
      return connection.SubscribeToAllAsync(settings, eventAppeared, subscriptionDropped, userCredentials);
    }

    /// <summary>Asynchronously subscribes to all events in the Event Store. New
    /// events written to the stream while the subscription is active
    /// will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnection"/> responsible for raising the event.</param>
    /// <param name="resolveLinkTos">Whether to resolve Link events automatically</param>
    /// <param name="eventAppearedAsync">A Task invoked and awaited when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <returns>A <see cref="Task&lt;EventStoreSubscription&gt;"/> representing the subscription.</returns>
    public static Task<EventStoreSubscription> SubscribeToAllAsync(this IEventStoreConnection connection, bool resolveLinkTos,
      Func<EventStoreSubscription, ResolvedEvent, Task> eventAppearedAsync,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null)
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      var settings = new SubscriptionSettings { ResolveLinkTos = resolveLinkTos };
      return connection.SubscribeToAllAsync(settings, eventAppearedAsync, subscriptionDropped, userCredentials);
    }

    #endregion

    #region -- VolatileSubscribe(NonGeneric) --

    /// <summary>Subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase2"/> responsible for raising the event.</param>
    /// <param name="stream">The stream to subscribe to</param>
    /// <param name="resolveLinkTos">Whether to resolve Link events automatically</param>
    /// <param name="eventAppeared">An action invoked when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <param name="verboseLogging">Enables verbose logging on the subscription</param>
    /// <returns>A <see cref="EventStoreSubscription"/> representing the subscription.</returns>
    public static EventStoreSubscription VolatileSubscribe(this IEventStoreConnectionBase2 connection, string stream, bool resolveLinkTos,
      Action<EventStoreSubscription, ResolvedEvent<object>> eventAppeared,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null, bool verboseLogging = false)
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      var subscriptionSettings = new SubscriptionSettings { ResolveLinkTos = resolveLinkTos, VerboseLogging = verboseLogging };
      return AsyncContext.Run(
        async (conn, streamId, settings, eAppeared, subDropped, credentials)
          => await conn.VolatileSubscribeAsync(streamId, settings, eAppeared, subDropped, credentials).ConfigureAwait(false),
        connection, stream, subscriptionSettings, eventAppeared, subscriptionDropped, userCredentials);
    }

    /// <summary>Subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase2"/> responsible for raising the event.</param>
    /// <param name="stream">The stream to subscribe to</param>
    /// <param name="resolveLinkTos">Whether to resolve Link events automatically</param>
    /// <param name="eventAppearedAsync">A Task invoked and awaited when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <param name="verboseLogging">Enables verbose logging on the subscription</param>
    /// <returns>A <see cref="EventStoreSubscription"/> representing the subscription.</returns>
    public static EventStoreSubscription VolatileSubscribe(this IEventStoreConnectionBase2 connection, string stream, bool resolveLinkTos,
      Func<EventStoreSubscription, ResolvedEvent<object>, Task> eventAppearedAsync,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null, bool verboseLogging = false)
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      var subscriptionSettings = new SubscriptionSettings { ResolveLinkTos = resolveLinkTos, VerboseLogging = verboseLogging };
      return AsyncContext.Run(
        async (conn, streamId, settings, eAppeared, subDropped, credentials)
          => await conn.VolatileSubscribeAsync(streamId, settings, eAppeared, subDropped, credentials).ConfigureAwait(false),
        connection, stream, subscriptionSettings, eventAppearedAsync, subscriptionDropped, userCredentials);
    }

    /// <summary>Subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase2"/> responsible for raising the event.</param>
    /// <param name="stream">The stream to subscribe to</param>
    /// <param name="subscriptionSettings">The <see cref="SubscriptionSettings"/> for the subscription</param>
    /// <param name="eventAppeared">An action invoked when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <returns>A <see cref="EventStoreSubscription"/> representing the subscription.</returns>
    public static EventStoreSubscription VolatileSubscribe(this IEventStoreConnectionBase2 connection,
      string stream, SubscriptionSettings subscriptionSettings,
      Action<EventStoreSubscription, ResolvedEvent<object>> eventAppeared,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null)
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      return AsyncContext.Run(
        async (conn, streamId, settings, eAppeared, subDropped, credentials)
          => await conn.VolatileSubscribeAsync(streamId, settings, eAppeared, subDropped, credentials).ConfigureAwait(false),
        connection, stream, subscriptionSettings, eventAppeared, subscriptionDropped, userCredentials);
    }

    /// <summary>Subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase2"/> responsible for raising the event.</param>
    /// <param name="stream">The stream to subscribe to</param>
    /// <param name="subscriptionSettings">The <see cref="SubscriptionSettings"/> for the subscription</param>
    /// <param name="eventAppearedAsync">A Task invoked and awaited when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <returns>A <see cref="EventStoreSubscription"/> representing the subscription.</returns>
    public static EventStoreSubscription VolatileSubscribe(this IEventStoreConnectionBase2 connection,
      string stream, SubscriptionSettings subscriptionSettings,
      Func<EventStoreSubscription, ResolvedEvent<object>, Task> eventAppearedAsync,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null)
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      return AsyncContext.Run(
        async (conn, streamId, settings, eAppeared, subDropped, credentials)
          => await conn.VolatileSubscribeAsync(streamId, settings, eAppeared, subDropped, credentials).ConfigureAwait(false),
        connection, stream, subscriptionSettings, eventAppearedAsync, subscriptionDropped, userCredentials);
    }

    #endregion

    #region -- VolatileSubscribe(NonGeneric-Topic) --

    /// <summary>Subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase2"/> responsible for raising the event.</param>
    /// <param name="stream">The stream to subscribe to</param>
    /// <param name="topic">The topic</param>
    /// <param name="resolveLinkTos">Whether to resolve Link events automatically</param>
    /// <param name="eventAppeared">An action invoked when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <param name="verboseLogging">Enables verbose logging on the subscription</param>
    /// <returns>A <see cref="EventStoreSubscription"/> representing the subscription.</returns>
    public static EventStoreSubscription VolatileSubscribe(this IEventStoreConnectionBase2 connection,
      string stream, string topic, bool resolveLinkTos,
      Action<EventStoreSubscription, ResolvedEvent<object>> eventAppeared,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null, bool verboseLogging = false)
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      if (string.IsNullOrEmpty(stream)) { throw new ArgumentNullException(nameof(stream)); }
      if (string.IsNullOrEmpty(topic)) { throw new ArgumentNullException(nameof(topic)); }
      var subscriptionSettings = new SubscriptionSettings { ResolveLinkTos = resolveLinkTos, VerboseLogging = verboseLogging };
      return AsyncContext.Run(
        async (conn, streamId, settings, eAppeared, subDropped, credentials)
          => await conn.VolatileSubscribeAsync(streamId, settings, eAppeared, subDropped, credentials).ConfigureAwait(false),
        connection, CombineStreamId(stream, topic), subscriptionSettings, eventAppeared, subscriptionDropped, userCredentials);
    }

    /// <summary>Subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase2"/> responsible for raising the event.</param>
    /// <param name="stream">The stream to subscribe to</param>
    /// <param name="topic">The topic</param>
    /// <param name="resolveLinkTos">Whether to resolve Link events automatically</param>
    /// <param name="eventAppearedAsync">A Task invoked and awaited when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <param name="verboseLogging">Enables verbose logging on the subscription</param>
    /// <returns>A <see cref="EventStoreSubscription"/> representing the subscription.</returns>
    public static EventStoreSubscription VolatileSubscribe(this IEventStoreConnectionBase2 connection,
      string stream, string topic, bool resolveLinkTos,
      Func<EventStoreSubscription, ResolvedEvent<object>, Task> eventAppearedAsync,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null, bool verboseLogging = false)
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      if (string.IsNullOrEmpty(stream)) { throw new ArgumentNullException(nameof(stream)); }
      if (string.IsNullOrEmpty(topic)) { throw new ArgumentNullException(nameof(topic)); }
      var subscriptionSettings = new SubscriptionSettings { ResolveLinkTos = resolveLinkTos, VerboseLogging = verboseLogging };
      return AsyncContext.Run(
        async (conn, streamId, settings, eAppeared, subDropped, credentials)
          => await conn.VolatileSubscribeAsync(streamId, settings, eAppeared, subDropped, credentials).ConfigureAwait(false),
        connection, CombineStreamId(stream, topic), subscriptionSettings, eventAppearedAsync, subscriptionDropped, userCredentials);
    }

    /// <summary>Subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase2"/> responsible for raising the event.</param>
    /// <param name="stream">The stream to subscribe to</param>
    /// <param name="topic">The topic</param>
    /// <param name="subscriptionSettings">The <see cref="SubscriptionSettings"/> for the subscription</param>
    /// <param name="eventAppeared">An action invoked when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <returns>A <see cref="EventStoreSubscription"/> representing the subscription.</returns>
    public static EventStoreSubscription VolatileSubscribe(this IEventStoreConnectionBase2 connection,
      string stream, string topic, SubscriptionSettings subscriptionSettings,
      Action<EventStoreSubscription, ResolvedEvent<object>> eventAppeared,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null)
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      if (string.IsNullOrEmpty(stream)) { throw new ArgumentNullException(nameof(stream)); }
      if (string.IsNullOrEmpty(topic)) { throw new ArgumentNullException(nameof(topic)); }
      return AsyncContext.Run(
        async (conn, streamId, settings, eAppeared, subDropped, credentials)
          => await conn.VolatileSubscribeAsync(streamId, settings, eAppeared, subDropped, credentials).ConfigureAwait(false),
        connection, CombineStreamId(stream, topic), subscriptionSettings, eventAppeared, subscriptionDropped, userCredentials);
    }

    /// <summary>Subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase2"/> responsible for raising the event.</param>
    /// <param name="stream">The stream to subscribe to</param>
    /// <param name="topic">The topic</param>
    /// <param name="subscriptionSettings">The <see cref="SubscriptionSettings"/> for the subscription</param>
    /// <param name="eventAppearedAsync">A Task invoked and awaited when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <returns>A <see cref="EventStoreSubscription"/> representing the subscription.</returns>
    public static EventStoreSubscription VolatileSubscribe(this IEventStoreConnectionBase2 connection,
      string stream, string topic, SubscriptionSettings subscriptionSettings,
      Func<EventStoreSubscription, ResolvedEvent<object>, Task> eventAppearedAsync,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null)
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      if (string.IsNullOrEmpty(stream)) { throw new ArgumentNullException(nameof(stream)); }
      if (string.IsNullOrEmpty(topic)) { throw new ArgumentNullException(nameof(topic)); }
      return AsyncContext.Run(
        async (conn, streamId, settings, eAppeared, subDropped, credentials)
          => await conn.VolatileSubscribeAsync(streamId, settings, eAppeared, subDropped, credentials).ConfigureAwait(false),
        connection, CombineStreamId(stream, topic), subscriptionSettings, eventAppearedAsync, subscriptionDropped, userCredentials);
    }

    #endregion

    #region -- VolatileSubscribe(Generic) --

    /// <summary>Subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase2"/> responsible for raising the event.</param>
    /// <param name="resolveLinkTos">Whether to resolve Link events automatically</param>
    /// <param name="eventAppeared">An action invoked when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <param name="verboseLogging">Enables verbose logging on the subscription</param>
    /// <returns>A <see cref="EventStoreSubscription"/> representing the subscription.</returns>
    public static EventStoreSubscription VolatileSubscribe<TEnent>(this IEventStoreConnectionBase2 connection, bool resolveLinkTos,
      Action<EventStoreSubscription, ResolvedEvent<TEnent>> eventAppeared,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null, bool verboseLogging = false) where TEnent : class
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      var subscriptionSettings = new SubscriptionSettings { ResolveLinkTos = resolveLinkTos, VerboseLogging = verboseLogging };
      return AsyncContext.Run(
        async (conn, settings, eAppeared, subDropped, credentials)
          => await conn.VolatileSubscribeAsync<TEnent>(settings, eAppeared, subDropped, credentials).ConfigureAwait(false),
        connection, subscriptionSettings, eventAppeared, subscriptionDropped, userCredentials);
    }

    /// <summary>Subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase2"/> responsible for raising the event.</param>
    /// <param name="resolveLinkTos">Whether to resolve Link events automatically</param>
    /// <param name="eventAppearedAsync">A Task invoked and awaited when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <param name="verboseLogging">Enables verbose logging on the subscription</param>
    /// <returns>A <see cref="EventStoreSubscription"/> representing the subscription.</returns>
    public static EventStoreSubscription VolatileSubscribe<TEnent>(this IEventStoreConnectionBase2 connection, bool resolveLinkTos,
      Func<EventStoreSubscription, ResolvedEvent<TEnent>, Task> eventAppearedAsync,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null, bool verboseLogging = false) where TEnent : class
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      var subscriptionSettings = new SubscriptionSettings { ResolveLinkTos = resolveLinkTos, VerboseLogging = verboseLogging };
      return AsyncContext.Run(
        async (conn, settings, eAppeared, subDropped, credentials)
          => await conn.VolatileSubscribeAsync<TEnent>(settings, eAppeared, subDropped, credentials).ConfigureAwait(false),
        connection, subscriptionSettings, eventAppearedAsync, subscriptionDropped, userCredentials);
    }

    /// <summary>Subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase2"/> responsible for raising the event.</param>
    /// <param name="subscriptionSettings">The <see cref="SubscriptionSettings"/> for the subscription</param>
    /// <param name="eventAppeared">An action invoked when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <returns>A <see cref="EventStoreSubscription"/> representing the subscription.</returns>
    public static EventStoreSubscription VolatileSubscribe<TEnent>(this IEventStoreConnectionBase2 connection, SubscriptionSettings subscriptionSettings,
      Action<EventStoreSubscription, ResolvedEvent<TEnent>> eventAppeared,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null) where TEnent : class
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      return AsyncContext.Run(
        async (conn, settings, eAppeared, subDropped, credentials)
          => await conn.VolatileSubscribeAsync<TEnent>(settings, eAppeared, subDropped, credentials).ConfigureAwait(false),
        connection, subscriptionSettings, eventAppeared, subscriptionDropped, userCredentials);
    }

    /// <summary>Subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase2"/> responsible for raising the event.</param>
    /// <param name="subscriptionSettings">The <see cref="SubscriptionSettings"/> for the subscription</param>
    /// <param name="eventAppearedAsync">A Task invoked and awaited when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <returns>A <see cref="EventStoreSubscription"/> representing the subscription.</returns>
    public static EventStoreSubscription VolatileSubscribe<TEnent>(this IEventStoreConnectionBase2 connection, SubscriptionSettings subscriptionSettings,
      Func<EventStoreSubscription, ResolvedEvent<TEnent>, Task> eventAppearedAsync,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null) where TEnent : class
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      return AsyncContext.Run(
        async (conn, settings, eAppeared, subDropped, credentials)
          => await conn.VolatileSubscribeAsync<TEnent>(settings, eAppeared, subDropped, credentials).ConfigureAwait(false),
        connection, subscriptionSettings, eventAppearedAsync, subscriptionDropped, userCredentials);
    }

    #endregion

    #region -- VolatileSubscribe(Generic-Topic) --

    /// <summary>Subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase2"/> responsible for raising the event.</param>
    /// <param name="topic">The topic</param>
    /// <param name="resolveLinkTos">Whether to resolve Link events automatically</param>
    /// <param name="eventAppeared">An action invoked when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <param name="verboseLogging">Enables verbose logging on the subscription</param>
    /// <returns>A <see cref="EventStoreSubscription"/> representing the subscription.</returns>
    public static EventStoreSubscription VolatileSubscribe<TEnent>(this IEventStoreConnectionBase2 connection, string topic, bool resolveLinkTos,
      Action<EventStoreSubscription, ResolvedEvent<TEnent>> eventAppeared,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null, bool verboseLogging = false) where TEnent : class
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      var subscriptionSettings = new SubscriptionSettings { ResolveLinkTos = resolveLinkTos, VerboseLogging = verboseLogging };
      return AsyncContext.Run(
        async (conn, innerTopic, settings, eAppeared, subDropped, credentials)
          => await conn.VolatileSubscribeAsync<TEnent>(innerTopic, settings, eAppeared, subDropped, credentials).ConfigureAwait(false),
        connection, topic, subscriptionSettings, eventAppeared, subscriptionDropped, userCredentials);
    }

    /// <summary>Subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase2"/> responsible for raising the event.</param>
    /// <param name="topic">The topic</param>
    /// <param name="resolveLinkTos">Whether to resolve Link events automatically</param>
    /// <param name="eventAppearedAsync">A Task invoked and awaited when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <param name="verboseLogging">Enables verbose logging on the subscription</param>
    /// <returns>A <see cref="EventStoreSubscription"/> representing the subscription.</returns>
    public static EventStoreSubscription VolatileSubscribe<TEnent>(this IEventStoreConnectionBase2 connection, string topic, bool resolveLinkTos,
      Func<EventStoreSubscription, ResolvedEvent<TEnent>, Task> eventAppearedAsync,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null, bool verboseLogging = false) where TEnent : class
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      var subscriptionSettings = new SubscriptionSettings { ResolveLinkTos = resolveLinkTos, VerboseLogging = verboseLogging };
      return AsyncContext.Run(
        async (conn, innerTopic, settings, eAppeared, subDropped, credentials)
          => await conn.VolatileSubscribeAsync<TEnent>(innerTopic, settings, eAppeared, subDropped, credentials).ConfigureAwait(false),
        connection, topic, subscriptionSettings, eventAppearedAsync, subscriptionDropped, userCredentials);
    }

    /// <summary>Subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase2"/> responsible for raising the event.</param>
    /// <param name="topic">The topic</param>
    /// <param name="subscriptionSettings">The <see cref="SubscriptionSettings"/> for the subscription</param>
    /// <param name="eventAppeared">An action invoked when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <returns>A <see cref="EventStoreSubscription"/> representing the subscription.</returns>
    public static EventStoreSubscription VolatileSubscribe<TEnent>(this IEventStoreConnectionBase2 connection,
      string topic, SubscriptionSettings subscriptionSettings,
      Action<EventStoreSubscription, ResolvedEvent<TEnent>> eventAppeared,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null) where TEnent : class
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      return AsyncContext.Run(
        async (conn, innerTopic, settings, eAppeared, subDropped, credentials)
          => await conn.VolatileSubscribeAsync<TEnent>(innerTopic, settings, eAppeared, subDropped, credentials).ConfigureAwait(false),
        connection, topic, subscriptionSettings, eventAppeared, subscriptionDropped, userCredentials);
    }

    /// <summary>Subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase2"/> responsible for raising the event.</param>
    /// <param name="topic">The topic</param>
    /// <param name="subscriptionSettings">The <see cref="SubscriptionSettings"/> for the subscription</param>
    /// <param name="eventAppearedAsync">A Task invoked and awaited when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <returns>A <see cref="EventStoreSubscription"/> representing the subscription.</returns>
    public static EventStoreSubscription VolatileSubscribe<TEnent>(this IEventStoreConnectionBase2 connection,
      string topic, SubscriptionSettings subscriptionSettings,
      Func<EventStoreSubscription, ResolvedEvent<TEnent>, Task> eventAppearedAsync,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null) where TEnent : class
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      return AsyncContext.Run(
        async (conn, innerTopic, settings, eAppeared, subDropped, credentials)
          => await conn.VolatileSubscribeAsync<TEnent>(innerTopic, settings, eAppeared, subDropped, credentials).ConfigureAwait(false),
        connection, topic, subscriptionSettings, eventAppearedAsync, subscriptionDropped, userCredentials);
    }

    #endregion

    #region -- VolatileSubscribeAsync(NonGeneric) --

    /// <summary>Asynchronously subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase2"/> responsible for raising the event.</param>
    /// <param name="stream">The stream to subscribe to</param>
    /// <param name="resolveLinkTos">Whether to resolve Link events automatically</param>
    /// <param name="eventAppeared">An action invoked when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <returns>A <see cref="Task&lt;EventStoreSubscription&gt;"/> representing the subscription.</returns>
    public static Task<EventStoreSubscription> VolatileSubscribeAsync(this IEventStoreConnectionBase2 connection, string stream, bool resolveLinkTos,
      Action<EventStoreSubscription, ResolvedEvent<object>> eventAppeared,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null)
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      var settings = new SubscriptionSettings { ResolveLinkTos = resolveLinkTos };
      return connection.VolatileSubscribeAsync(stream, settings, eventAppeared, subscriptionDropped, userCredentials);
    }

    /// <summary>Asynchronously subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase2"/> responsible for raising the event.</param>
    /// <param name="stream">The stream to subscribe to</param>
    /// <param name="resolveLinkTos">Whether to resolve Link events automatically</param>
    /// <param name="eventAppearedAsync">A Task invoked and awaited when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <returns>A <see cref="Task&lt;EventStoreSubscription&gt;"/> representing the subscription.</returns>
    public static Task<EventStoreSubscription> VolatileSubscribeAsync(this IEventStoreConnectionBase2 connection, string stream, bool resolveLinkTos,
      Func<EventStoreSubscription, ResolvedEvent<object>, Task> eventAppearedAsync,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null)
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      var settings = new SubscriptionSettings { ResolveLinkTos = resolveLinkTos };
      return connection.VolatileSubscribeAsync(stream, settings, eventAppearedAsync, subscriptionDropped, userCredentials);
    }

    #endregion

    #region -- VolatileSubscribeAsync(Generic) --

    /// <summary>Asynchronously subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase2"/> responsible for raising the event.</param>
    /// <param name="resolveLinkTos">Whether to resolve Link events automatically</param>
    /// <param name="eventAppeared">An action invoked when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <returns>A <see cref="Task&lt;EventStoreSubscription&gt;"/> representing the subscription.</returns>
    public static Task<EventStoreSubscription> VolatileSubscribeAsync<TEvent>(this IEventStoreConnectionBase2 connection, bool resolveLinkTos,
      Action<EventStoreSubscription, ResolvedEvent<TEvent>> eventAppeared,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null) where TEvent : class
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      var settings = new SubscriptionSettings { ResolveLinkTos = resolveLinkTos };
      return connection.VolatileSubscribeAsync<TEvent>(settings, eventAppeared, subscriptionDropped, userCredentials);
    }

    /// <summary>Asynchronously subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase2"/> responsible for raising the event.</param>
    /// <param name="resolveLinkTos">Whether to resolve Link events automatically</param>
    /// <param name="eventAppearedAsync">A Task invoked and awaited when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <returns>A <see cref="Task&lt;EventStoreSubscription&gt;"/> representing the subscription.</returns>
    public static Task<EventStoreSubscription> VolatileSubscribeAsync<TEvent>(this IEventStoreConnectionBase2 connection, bool resolveLinkTos,
      Func<EventStoreSubscription, ResolvedEvent<TEvent>, Task> eventAppearedAsync,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null) where TEvent : class
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      var settings = new SubscriptionSettings { ResolveLinkTos = resolveLinkTos };
      return connection.VolatileSubscribeAsync<TEvent>(settings, eventAppearedAsync, subscriptionDropped, userCredentials);
    }

    #endregion

    #region -- VolatileSubscribeAsync(NonGeneric-Topic) --

    /// <summary>Asynchronously subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase2"/> responsible for raising the event.</param>
    /// <param name="stream">The stream to subscribe to</param>
    /// <param name="topic">The topic</param>
    /// <param name="resolveLinkTos">Whether to resolve Link events automatically</param>
    /// <param name="eventAppeared">An action invoked when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <returns>A <see cref="Task&lt;EventStoreSubscription&gt;"/> representing the subscription.</returns>
    public static Task<EventStoreSubscription> VolatileSubscribeAsync(this IEventStoreConnectionBase2 connection,
      string stream, string topic, bool resolveLinkTos,
      Action<EventStoreSubscription, ResolvedEvent<object>> eventAppeared,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null)
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      if (string.IsNullOrEmpty(stream)) { throw new ArgumentNullException(nameof(stream)); }
      if (string.IsNullOrEmpty(topic)) { throw new ArgumentNullException(nameof(topic)); }
      var settings = new SubscriptionSettings { ResolveLinkTos = resolveLinkTos };
      return connection.VolatileSubscribeAsync(CombineStreamId(stream, topic), settings, eventAppeared, subscriptionDropped, userCredentials);
    }

    /// <summary>Asynchronously subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase2"/> responsible for raising the event.</param>
    /// <param name="stream">The stream to subscribe to</param>
    /// <param name="topic">The topic</param>
    /// <param name="resolveLinkTos">Whether to resolve Link events automatically</param>
    /// <param name="eventAppearedAsync">A Task invoked and awaited when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <returns>A <see cref="Task&lt;EventStoreSubscription&gt;"/> representing the subscription.</returns>
    public static Task<EventStoreSubscription> VolatileSubscribeAsync(this IEventStoreConnectionBase2 connection,
      string stream, string topic, bool resolveLinkTos,
      Func<EventStoreSubscription, ResolvedEvent<object>, Task> eventAppearedAsync,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null)
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      if (string.IsNullOrEmpty(stream)) { throw new ArgumentNullException(nameof(stream)); }
      if (string.IsNullOrEmpty(topic)) { throw new ArgumentNullException(nameof(topic)); }
      var settings = new SubscriptionSettings { ResolveLinkTos = resolveLinkTos };
      return connection.VolatileSubscribeAsync(CombineStreamId(stream, topic), settings, eventAppearedAsync, subscriptionDropped, userCredentials);
    }

    /// <summary>Asynchronously subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase2"/> responsible for raising the event.</param>
    /// <param name="stream">The stream to subscribe to</param>
    /// <param name="topic">The topic</param>
    /// <param name="settings">The <see cref="SubscriptionSettings"/> for the subscription</param>
    /// <param name="eventAppeared">An action invoked when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <returns>A <see cref="Task&lt;EventStoreSubscription&gt;"/> representing the subscription.</returns>
    public static Task<EventStoreSubscription> VolatileSubscribeAsync(this IEventStoreConnectionBase2 connection,
      string stream, string topic, SubscriptionSettings settings,
      Action<EventStoreSubscription, ResolvedEvent<object>> eventAppeared,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null)
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      if (string.IsNullOrEmpty(stream)) { throw new ArgumentNullException(nameof(stream)); }
      if (string.IsNullOrEmpty(topic)) { throw new ArgumentNullException(nameof(topic)); }
      return connection.VolatileSubscribeAsync(CombineStreamId(stream, topic), settings, eventAppeared, subscriptionDropped, userCredentials);
    }

    /// <summary>Asynchronously subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase2"/> responsible for raising the event.</param>
    /// <param name="stream">The stream to subscribe to</param>
    /// <param name="topic">The topic</param>
    /// <param name="settings">The <see cref="SubscriptionSettings"/> for the subscription</param>
    /// <param name="eventAppearedAsync">A Task invoked and awaited when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <returns>A <see cref="Task&lt;EventStoreSubscription&gt;"/> representing the subscription.</returns>
    public static Task<EventStoreSubscription> VolatileSubscribeAsync(this IEventStoreConnectionBase2 connection,
      string stream, string topic, SubscriptionSettings settings,
      Func<EventStoreSubscription, ResolvedEvent<object>, Task> eventAppearedAsync,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null)
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      if (string.IsNullOrEmpty(stream)) { throw new ArgumentNullException(nameof(stream)); }
      if (string.IsNullOrEmpty(topic)) { throw new ArgumentNullException(nameof(topic)); }
      return connection.VolatileSubscribeAsync(CombineStreamId(stream, topic), settings, eventAppearedAsync, subscriptionDropped, userCredentials);
    }

    #endregion

    #region -- VolatileSubscribeAsync(Generic-Topic) --

    /// <summary>Asynchronously subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase2"/> responsible for raising the event.</param>
    /// <param name="topic">The topic</param>
    /// <param name="resolveLinkTos">Whether to resolve Link events automatically</param>
    /// <param name="eventAppeared">An action invoked when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <returns>A <see cref="Task&lt;EventStoreSubscription&gt;"/> representing the subscription.</returns>
    public static Task<EventStoreSubscription> VolatileSubscribeAsync<TEvent>(this IEventStoreConnectionBase2 connection, string topic, bool resolveLinkTos,
      Action<EventStoreSubscription, ResolvedEvent<TEvent>> eventAppeared,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null) where TEvent : class
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      var settings = new SubscriptionSettings { ResolveLinkTos = resolveLinkTos };
      return connection.VolatileSubscribeAsync<TEvent>(topic, settings, eventAppeared, subscriptionDropped, userCredentials);
    }

    /// <summary>Asynchronously subscribes to a single event stream. New events
    /// written to the stream while the subscription is active will be pushed to the client.</summary>
    /// <param name="connection">The <see cref="IEventStoreConnectionBase2"/> responsible for raising the event.</param>
    /// <param name="topic">The topic</param>
    /// <param name="resolveLinkTos">Whether to resolve Link events automatically</param>
    /// <param name="eventAppearedAsync">A Task invoked and awaited when a new event is received over the subscription</param>
    /// <param name="subscriptionDropped">An action invoked if the subscription is dropped</param>
    /// <param name="userCredentials">User credentials to use for the operation</param>
    /// <returns>A <see cref="Task&lt;EventStoreSubscription&gt;"/> representing the subscription.</returns>
    public static Task<EventStoreSubscription> VolatileSubscribeAsync<TEvent>(this IEventStoreConnectionBase2 connection, string topic, bool resolveLinkTos,
      Func<EventStoreSubscription, ResolvedEvent<TEvent>, Task> eventAppearedAsync,
      Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
      UserCredentials userCredentials = null) where TEvent : class
    {
      if (null == connection) { throw new ArgumentNullException(nameof(connection)); }
      var settings = new SubscriptionSettings { ResolveLinkTos = resolveLinkTos };
      return connection.VolatileSubscribeAsync<TEvent>(topic, settings, eventAppearedAsync, subscriptionDropped, userCredentials);
    }

    #endregion
  }
}
