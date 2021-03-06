﻿using System;

namespace EventStore.ClientAPI
{
  /// <summary>A Event Read Result is the result of a single event read operation to the Event Store.</summary>
  public class EventReadResult
  {
    /// <summary>The <see cref="EventReadStatus"/> representing the status of this read attempt.</summary>
    public readonly EventReadStatus Status;

    /// <summary>The name of the stream read.</summary>
    public readonly string Stream;

    /// <summary>The event number of the requested event.</summary>
    public readonly long EventNumber;

    /// <summary>The event read represented as <see cref="ResolvedEvent"/>.</summary>
    public readonly ResolvedEvent? Event;

    internal EventReadResult(EventReadStatus status, string stream, long eventNumber, ResolvedEvent? @event)
    {
      if (string.IsNullOrEmpty(stream)) { throw new ArgumentNullException(nameof(stream)); }

      Status = status;
      Stream = stream;
      EventNumber = eventNumber;
      Event = @event;
    }

    //internal EventReadResult(EventReadStatus status, string stream, long eventNumber, ClientMessage.ResolvedIndexedEvent @event)
    //{
    //  Ensure.NotNullOrEmpty(stream, "stream");

    //  Status = status;
    //  Stream = stream;
    //  EventNumber = eventNumber;
    //  Event = status == EventReadStatus.Success ? new ResolvedEvent(@event) : (ResolvedEvent?)null;
    //}
  }
}