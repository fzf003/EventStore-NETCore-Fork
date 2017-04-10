﻿using EventStore.Core.Index;
using NUnit.Framework;

namespace EventStore.Core.Tests.Index.IndexV3
{
    [TestFixture]
    public class when_merging_two_ptables: IndexV1.when_merging_two_ptables
    {
        public when_merging_two_ptables()
        {
            _ptableVersion = PTableVersions.IndexV3;
        }
    }
}