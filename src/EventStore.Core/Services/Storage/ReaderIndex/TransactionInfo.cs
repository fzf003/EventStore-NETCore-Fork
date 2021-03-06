﻿namespace EventStore.Core.Services.Storage.ReaderIndex
{
    public readonly struct TransactionInfo
    {
        public readonly int TransactionOffset;
        public readonly string EventStreamId;

        public TransactionInfo(int transactionOffset, string eventStreamId)
        {
            TransactionOffset = transactionOffset;
            EventStreamId = eventStreamId;
        }
    }
}