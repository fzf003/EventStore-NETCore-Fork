﻿#if false
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using EventStore.ClientAPI.SystemData;
using Microsoft.Extensions.Logging;

namespace EventStore.ClientAPI
{
  /// <summary>Represents a persistent subscription connection.</summary>
  public abstract class EventStorePersistentSubscriptionBase
  {
    ///<summary>The default buffer size for the persistent subscription</summary>
    public const int DefaultBufferSize = 10;

    private readonly string _subscriptionId;
    private readonly string _streamId;
    private readonly Action<EventStorePersistentSubscriptionBase, ResolvedEvent> _eventAppeared;
    private readonly Action<EventStorePersistentSubscriptionBase, SubscriptionDropReason, Exception> _subscriptionDropped;
    private readonly UserCredentials _userCredentials;
    private readonly ILogger _log;
    private readonly bool _verbose;
    private readonly ConnectionSettings _settings;
    private readonly bool _autoAck;

    private PersistentEventStoreSubscription _subscription;
    private ActionBlock<ResolvedEvent> _resolvedEventBlock;
    private DropData _dropData;

    private int _isDropped;
    private readonly int _bufferSize;

    internal EventStorePersistentSubscriptionBase(string subscriptionId,
      string streamId,
      Action<EventStorePersistentSubscriptionBase, ResolvedEvent> eventAppeared,
      Action<EventStorePersistentSubscriptionBase, SubscriptionDropReason, Exception> subscriptionDropped,
      UserCredentials userCredentials,
      ILogger log,
      bool verboseLogging,
      ConnectionSettings settings,
      int bufferSize = 10,
      bool autoAck = true)
    {
      _subscriptionId = subscriptionId;
      _streamId = streamId;
      _eventAppeared = eventAppeared;
      _subscriptionDropped = subscriptionDropped;
      _userCredentials = userCredentials;
      _log = log;
      _verbose = verboseLogging;
      _settings = settings;
      _bufferSize = bufferSize;
      _autoAck = autoAck;
    }

    internal async Task<EventStorePersistentSubscriptionBase> Start()
    {
      _subscription = await StartSubscription(_subscriptionId, _streamId, _bufferSize, _userCredentials, OnEventAppeared, OnSubscriptionDropped, _settings);
      InitResolvedEventBlock();

      return this;
    }

    private void InitResolvedEventBlock()
    {
      _resolvedEventBlock = new ActionBlock<ResolvedEvent>(new Action<ResolvedEvent>(ProcessResolvedEvent));
      var completionTask = _resolvedEventBlock.Completion;
      completionTask.ContinueWith((task, state) =>
        {
          var dropData = state as DropData;
          if (dropData == null)
          {
            //throw new Exception("Drop reason not specified.");
            dropData = new DropData(SubscriptionDropReason.Unknown, new Exception("Drop reason not specified."));
          }
          DropSubscription(dropData.Reason, dropData.Error);
        },
        _dropData,
        CancellationToken.None,
        TaskContinuationOptions.DenyChildAttach | TaskContinuationOptions.ExecuteSynchronously,
        TaskScheduler.Default
      );
    }

    internal abstract Task<PersistentEventStoreSubscription> StartSubscription(
        string subscriptionId, string streamId, int bufferSize, UserCredentials userCredentials,
        Action<EventStoreSubscription, ResolvedEvent> onEventAppeared,
        Action<EventStoreSubscription, SubscriptionDropReason, Exception> onSubscriptionDropped,
        ConnectionSettings settings);

    /// <summary>Acknowledge that a message have completed processing (this will tell the server it has been processed).</summary>
    /// <remarks>There is no need to ack a message if you have Auto Ack enabled</remarks>
    /// <param name="event">The <see cref="ResolvedEvent"></see> to acknowledge</param>
    public void Acknowledge(ResolvedEvent @event)
    {
      _subscription.NotifyEventsProcessed(new[] { @event.OriginalEvent.EventId });
    }

    /// <summary>Acknowledge that a message have completed processing (this will tell the server it has been processed).</summary>
    /// <remarks>There is no need to ack a message if you have Auto Ack enabled</remarks>
    /// <param name="events">The <see cref="ResolvedEvent"></see>s to acknowledge there should be less than 2000 to ack at a time.</param>
    public void Acknowledge(IEnumerable<ResolvedEvent> events)
    {
      var ids = events.Select(x => x.OriginalEvent.EventId).ToArray();
      if (ids.Length > 2000) throw new ArgumentOutOfRangeException(nameof(events), "events is limited to 2000 to ack at a time");
      _subscription.NotifyEventsProcessed(ids);
    }

    /// <summary>Acknowledge a message by event id (this will tell the server it has been processed).</summary>
    /// <remarks>There is no need to ack a message if you have Auto Ack enabled</remarks>
    /// <param name="eventId">The <see cref="ResolvedEvent"></see> OriginalEvent.EventId to acknowledge</param>
    public void Acknowledge(Guid eventId)
    {
      _subscription.NotifyEventsProcessed(new[] { eventId });
    }

    /// <summary>Acknowledge a group of messages by event id (this will tell the server it has been processed).</summary>
    /// <remarks>There is no need to ack a message if you have Auto Ack enabled</remarks>
    /// <param name="events">The <see cref="ResolvedEvent"></see> OriginalEvent.EventIds to acknowledge there should be less than 2000 to ack at a time.</param>
    public void Acknowledge(IEnumerable<Guid> events)
    {
      var ids = events.ToArray();
      if (ids.Length > 2000) throw new ArgumentOutOfRangeException(nameof(events), "events is limited to 2000 to ack at a time");
      _subscription.NotifyEventsProcessed(ids);
    }
    /// <summary>Mark a message failed processing. The server will be take action based upon the action paramter.</summary>
    /// <param name="event">The event to mark as failed</param>
    /// <param name="action">The <see cref="PersistentSubscriptionNakEventAction"></see> action to take</param>
    /// <param name="reason">A string with a message as to why the failure is occurring</param>
    public void Fail(ResolvedEvent @event, PersistentSubscriptionNakEventAction action, string reason)
    {
      _subscription.NotifyEventsFailed(new[] { @event.OriginalEvent.EventId }, action, reason);
    }

    /// <summary>Mark nmessages that have failed processing. The server will take action based upon the action parameter.</summary>
    /// <param name="events">The events to mark as failed</param>
    /// <param name="action">The <see cref="PersistentSubscriptionNakEventAction"></see> action to take</param>
    /// <param name="reason">A string with a message as to why the failure is occurring</param>
    public void Fail(IEnumerable<ResolvedEvent> events, PersistentSubscriptionNakEventAction action, string reason)
    {
      var ids = events.Select(x => x.OriginalEvent.EventId).ToArray();
      if (ids.Length > 2000) throw new ArgumentOutOfRangeException(nameof(events), "events is limited to 2000 to ack at a time");
      _subscription.NotifyEventsFailed(ids, action, reason);
    }

    /// <summary>Disconnects this client from the persistent subscriptions.</summary>
    /// <param name="timeout"></param>
    /// <exception cref="TimeoutException"></exception>
    public void Stop(TimeSpan timeout)
    {
      if (_verbose && _log.IsDebugLevelEnabled())
      {
        _log.LogDebug("Persistent Subscription to {0}: requesting stop...", _streamId);
      }

      EnqueueSubscriptionDropNotification(SubscriptionDropReason.UserInitiated, null);

      var completion = _resolvedEventBlock.Completion;
      if (!Task.WaitAll(new[] { completion }, timeout))
      {
        throw new TimeoutException($"Could not stop {GetType().Name} in time.");
      }
    }

    private void EnqueueSubscriptionDropNotification(SubscriptionDropReason reason, Exception error)
    {
      // if drop data was already set -- no need to enqueue drop again, somebody did that already
      var dropData = new DropData(reason, error);
      if (Interlocked.CompareExchange(ref _dropData, dropData, null) == null)
      {
        _resolvedEventBlock.Complete();
      }
    }

    private void OnSubscriptionDropped(EventStoreSubscription subscription, SubscriptionDropReason reason, Exception exception)
    {
      EnqueueSubscriptionDropNotification(reason, exception);
    }

    private void OnEventAppeared(EventStoreSubscription subscription, ResolvedEvent resolvedEvent)
    {
      _resolvedEventBlock.Post(resolvedEvent);
    }

    private void ProcessResolvedEvent(ResolvedEvent resolvedEvent)
    {
      try
      {
        _eventAppeared(this, resolvedEvent);
        if (_autoAck)
        {
          _subscription.NotifyEventsProcessed(new[] { resolvedEvent.OriginalEvent.EventId });
        }
        if (_verbose && _log.IsDebugLevelEnabled())
        {
          _log.LogDebug("Persistent Subscription to {0}: processed event ({1}, {2}, {3} @ {4}).",
                    _streamId,
                    resolvedEvent.OriginalEvent.EventStreamId, resolvedEvent.OriginalEvent.EventNumber, resolvedEvent.OriginalEvent.EventType, resolvedEvent.OriginalEventNumber);
        }
      }
      catch (Exception exc)
      {
        // TODO GFY should we autonak here?
        DropSubscription(SubscriptionDropReason.EventHandlerException, exc);
        return;
      }
    }

    private void DropSubscription(SubscriptionDropReason reason, Exception error)
    {
      if (Interlocked.CompareExchange(ref _isDropped, 1, 0) == 0)
      {
        if (_verbose && _log.IsDebugLevelEnabled())
        {
          _log.LogDebug("Persistent Subscription to {0}: dropping subscription, reason: {1} {2}.",
                    _streamId, reason, error == null ? string.Empty : error.ToString());
        }

        _subscription?.Unsubscribe();
        _subscriptionDropped?.Invoke(this, reason, error);
      }
    }

    private class DropData
    {
      public readonly SubscriptionDropReason Reason;
      public readonly Exception Error;

      public DropData(SubscriptionDropReason reason, Exception error)
      {
        Reason = reason;
        Error = error;
      }
    }
  }
}
#endif