﻿using System;
using EventStore.Projections.Core.Services;
using EventStore.Projections.Core.Services.Processing;
using EventStore.Projections.Core.Tests.Services.projections_manager;
using NUnit.Framework;

namespace EventStore.Projections.Core.Tests.Services.v8
{
    [TestFixture]
    public class when_not_returning_state_from_a_js_handler : TestFixtureWithJsProjection
    {
        protected override void Given()
        {
            _projection = @"
                fromAll().when({$any: function(state, event) {
                    state.newValue = 'new';
                }});
            ";
        }

        [Test, Category("v8")]
        public void process_event_should_return_updated_state()
        {
            string state;
            EmittedEventEnvelope[] emittedEvents;
            _stateHandler.ProcessEvent(
                "", CheckpointTag.FromPosition(0, 20, 10), "stream1", "type1", "category",
                Guid.NewGuid(), 0, "metadata", @"{""a"":""b""}", out state, out emittedEvents);
            Assert.IsTrue(state.Contains("\"newValue\":\"new\""));
        }

        [Test, Category("v8")]
        public void process_event_returns_true()
        {
            string state;
            EmittedEventEnvelope[] emittedEvents;
            var result = _stateHandler.ProcessEvent(
                "", CheckpointTag.FromPosition(0, 20, 10), "stream1", "type1", "category",
                Guid.NewGuid(), 0, "metadata", @"{""a"":""b""}", out state, out emittedEvents);

            Assert.IsTrue(result);
        }

    }
}
