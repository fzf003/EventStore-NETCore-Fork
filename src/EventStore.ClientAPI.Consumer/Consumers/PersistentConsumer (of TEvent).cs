﻿using System;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI.Subscriptions;
using Microsoft.Extensions.Logging;

namespace EventStore.ClientAPI.Consumers
{
  /// <summary>Represents the consumer of a persistent subscription to EventStore: http://docs.geteventstore.com/introduction/4.0.0/subscriptions/
  /// This kind of consumer supports the competing consumer messaging pattern: http://www.enterpriseintegrationpatterns.com/patterns/messaging/CompetingConsumers.html </summary>
  public class PersistentConsumer<TEvent> : StreamConsumer<PersistentSubscription<TEvent>, ConnectToPersistentSubscriptionSettings, TEvent>
    where TEvent : class
  {
    private static readonly ILogger s_logger = TraceLogger.GetLogger<PersistentConsumer>();

    private Func<EventStorePersistentSubscription<TEvent>, ResolvedEvent<TEvent>, int?, Task> _resolvedEventAppearedAsync;
    private Action<EventStorePersistentSubscription<TEvent>, ResolvedEvent<TEvent>, int?> _resolvedEventAppeared;

    private EventStorePersistentSubscription<TEvent> esSubscription;

    protected override void OnDispose(bool disposing)
    {
      base.OnDispose(disposing);
      var subscription = Interlocked.Exchange(ref esSubscription, null);
      subscription?.Stop(TimeSpan.FromMinutes(1));
    }

    protected override void Initialize(IEventStoreBus bus, PersistentSubscription<TEvent> subscription)
    {
      if (string.IsNullOrEmpty(subscription.SubscriptionId)) { throw new ArgumentNullException(nameof(subscription.SubscriptionId)); }
      if (null == subscription.PersistentSettings) { throw new ArgumentNullException(nameof(subscription.PersistentSettings)); }

      base.Initialize(bus, subscription);
    }

    public void Initialize(IEventStoreBus bus, PersistentSubscription<TEvent> subscription,
      Func<EventStorePersistentSubscription<TEvent>, ResolvedEvent<TEvent>, int?, Task> resolvedEventAppearedAsync)
    {
      Initialize(bus, subscription);
      _resolvedEventAppearedAsync = resolvedEventAppearedAsync ?? throw new ArgumentNullException(nameof(resolvedEventAppearedAsync));
    }

    public void Initialize(IEventStoreBus bus, PersistentSubscription<TEvent> subscription,
      Action<EventStorePersistentSubscription<TEvent>, ResolvedEvent<TEvent>, int?> resolvedEventAppeared)
    {
      Initialize(bus, subscription);
      _resolvedEventAppeared = resolvedEventAppeared ?? throw new ArgumentNullException(nameof(resolvedEventAppeared));
    }

    public void Initialize(IEventStoreBus bus, PersistentSubscription<TEvent> subscription, Func<TEvent, Task> eventAppearedAsync)
    {
      if (null == eventAppearedAsync) { throw new ArgumentNullException(nameof(eventAppearedAsync)); }
      Initialize(bus, subscription);
      _resolvedEventAppearedAsync = (sub, resolvedEvent, count) => eventAppearedAsync(resolvedEvent.Body);
    }

    public void Initialize(IEventStoreBus bus, PersistentSubscription<TEvent> subscription, Action<TEvent> eventAppeared)
    {
      if (null == eventAppeared) { throw new ArgumentNullException(nameof(eventAppeared)); }
      Initialize(bus, subscription);
      _resolvedEventAppeared = (sub, resolvedEvent, count) => eventAppeared(resolvedEvent.Body);
    }

    public override async Task ConnectToSubscriptionAsync()
    {
      if (Interlocked.CompareExchange(ref _subscribed, ON, OFF) == ON) { return; }

      if (string.IsNullOrEmpty(Subscription.Topic))
      {
        Bus.UpdatePersistentSubscription<TEvent>(Subscription.SubscriptionId, Subscription.PersistentSettings, Subscription.Credentials);
      }
      else
      {
        Bus.UpdatePersistentSubscription<TEvent>(Subscription.Topic, Subscription.SubscriptionId, Subscription.PersistentSettings, Subscription.Credentials);
      }

      await InternalConnectToSubscriptionAsync().ConfigureAwait(false);
    }

    public override async Task ConnectToSubscriptionAsync(long? lastCheckpoint)
    {
      if (Interlocked.CompareExchange(ref _subscribed, ON, OFF) == ON) { return; }

      if (string.IsNullOrEmpty(Subscription.Topic))
      {
        Bus.DeletePersistentSubscription<TEvent>(Subscription.SubscriptionId, Subscription.Credentials);

        await Bus
            .CreatePersistentSubscriptionAsync<TEvent>(Subscription.SubscriptionId,
                Subscription.PersistentSettings.Clone(lastCheckpoint ?? -1), Subscription.Credentials)
            .ConfigureAwait(false);
      }
      else
      {
        Bus.DeletePersistentSubscription<TEvent>(Subscription.Topic, Subscription.SubscriptionId, Subscription.Credentials);

        await Bus
            .CreatePersistentSubscriptionAsync<TEvent>(Subscription.Topic, Subscription.SubscriptionId,
                Subscription.PersistentSettings.Clone(lastCheckpoint ?? -1), Subscription.Credentials)
            .ConfigureAwait(false);
      }

      await InternalConnectToSubscriptionAsync().ConfigureAwait(false);
    }

    private async Task InternalConnectToSubscriptionAsync()
    {
      try
      {
        if (_resolvedEventAppearedAsync != null)
        {
          esSubscription = await Bus.PersistentSubscribeAsync<TEvent>(Subscription.Topic, Subscription.SubscriptionId, Subscription.Settings, _resolvedEventAppearedAsync,
                  async (sub, reason, exception) => await SubscriptionDroppedAsync(sub, reason, exception).ConfigureAwait(false),
                  Subscription.Credentials).ConfigureAwait(false);
        }
        else
        {
          esSubscription = await Bus.PersistentSubscribeAsync<TEvent>(Subscription.Topic, Subscription.SubscriptionId, Subscription.Settings, _resolvedEventAppeared,
                  async (sub, reason, exception) => await SubscriptionDroppedAsync(sub, reason, exception).ConfigureAwait(false),
                  Subscription.Credentials).ConfigureAwait(false);
        }
      }
      catch (Exception exc)
      {
        s_logger.LogError(exc.ToString());
      }
    }

    private async Task SubscriptionDroppedAsync(EventStorePersistentSubscription<TEvent> subscription, SubscriptionDropReason dropReason, Exception exception)
    {
      if (await CanRetryAsync(subscription.ProcessingEventNumber, dropReason).ConfigureAwait(false))
      {
        var subscriptionDropped = new DroppedSubscription(Subscription, exception.Message, dropReason);
        await HandleDroppedSubscriptionAsync(subscriptionDropped).ConfigureAwait(false);
      }
    }
  }
}
