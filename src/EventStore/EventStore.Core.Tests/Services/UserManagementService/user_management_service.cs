﻿// Copyright (c) 2012, Event Store LLP
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
// 
// Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
// Redistributions in binary form must reproduce the above copyright
// notice, this list of conditions and the following disclaimer in the
// documentation and/or other materials provided with the distribution.
// Neither the name of the Event Store LLP nor the names of its
// contributors may be used to endorse or promote products derived from
// this software without specific prior written permission
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// 

using System.Linq;
using EventStore.Core.Messages;
using EventStore.Core.Messaging;
using EventStore.Core.Tests.Helper;
using EventStore.Core.Tests.Services.Transport.Http.Authentication;
using NUnit.Framework;

namespace EventStore.Core.Tests.Services.UserManagementService
{
    public static class user_management_service
    {
        public class TestFixtureWithUserManagementService : TestFixtureWithExistingEvents
        {
            protected Core.Services.UserManagement.UserManagementService _users;
            protected PublishEnvelope _replyTo;

            protected override void Given()
            {
                base.Given();
                NoStream("$user-user1");
                NoStream("$user-user2");
                NoStream("$user-user3");
                AllWritesSucceed();

                _replyTo = new PublishEnvelope(_bus);
                _users = new Core.Services.UserManagement.UserManagementService(
                    _bus, _ioDispatcher, new StubPasswordHashAlgorithm());
            }

            [SetUp]
            public void SetUp()
            {
                GivenCommands();
                _consumer.HandledMessages.Clear();
                When();
            }

            protected virtual void GivenCommands()
            {
            }

            protected virtual void When()
            {
            }
        }

        [TestFixture]
        public class when_creating_a_user : TestFixtureWithUserManagementService
        {
            protected override void When()
            {
                base.When();
                _users.Handle(
                    new UserManagementMessage.Create(
                        _replyTo, "user1", "John Doe", new[] {"admin", "other"}, "Johny123!"));
            }

            [Test]
            public void replies_success()
            {
                var updateResults = _consumer.HandledMessages.OfType<UserManagementMessage.UpdateResult>().ToList();
                Assert.AreEqual(1, updateResults.Count);
                Assert.IsTrue(updateResults[0].Success);
            }

            [Test]
            public void reply_has_the_correct_login_name()
            {
                var updateResults = _consumer.HandledMessages.OfType<UserManagementMessage.UpdateResult>().ToList();
                Assert.AreEqual(1, updateResults.Count);
                Assert.AreEqual("user1", updateResults[0].LoginName);
            }

            [Test]
            public void creates_an_enabled_user_account_with_correct_details()
            {
                _users.Handle(new UserManagementMessage.Get(_replyTo, "user1"));
                var user = _consumer.HandledMessages.OfType<UserManagementMessage.UserDetailsResult>().SingleOrDefault();
                Assert.NotNull(user);
                Assert.AreEqual("John Doe", user.Data.FullName);
                Assert.AreEqual("user1", user.Data.LoginName);
                Assert.NotNull(user.Data.Groups);
                Assert.That(user.Data.Groups.Any(v => v == "admin"));
                Assert.That(user.Data.Groups.Any(v => v == "other"));
                Assert.AreEqual(false, user.Data.Disabled);
            }

            [Test]
            public void creates_an_enabled_user_account_with_the_correct_password()
            {
                _consumer.HandledMessages.Clear();
                _users.Handle(new UserManagementMessage.ChangePassword(_replyTo, "user1", "Johny123!", "new-password"));
                var updateResult = _consumer.HandledMessages.OfType<UserManagementMessage.UpdateResult>().Last();
                Assert.NotNull(updateResult);
                Assert.IsTrue(updateResult.Success);
            }
        }

        [TestFixture]
        public class when_creating_an_already_existing_user_account : TestFixtureWithUserManagementService
        {
            protected override void GivenCommands()
            {
                base.GivenCommands();
                _users.Handle(
                    new UserManagementMessage.Create(
                        _replyTo, "user1", "Existing John", new[] {"admin", "other"}, "existing!"));
            }

            protected override void When()
            {
                base.When();
                _users.Handle(
                    new UserManagementMessage.Create(_replyTo, "user1", "John Doe", new[] {"bad"}, "Johny123!"));
            }

            [Test]
            public void replies_conflict()
            {
                var updateResults = _consumer.HandledMessages.OfType<UserManagementMessage.UpdateResult>().ToList();
                Assert.AreEqual(1, updateResults.Count);
                Assert.IsFalse(updateResults[0].Success);
                Assert.AreEqual(UserManagementMessage.Error.Conflict, updateResults[0].Error);
            }

            [Test]
            public void does_not_override_user_details()
            {
                _users.Handle(new UserManagementMessage.Get(_replyTo, "user1"));
                var user = _consumer.HandledMessages.OfType<UserManagementMessage.UserDetailsResult>().SingleOrDefault();
                Assert.NotNull(user);
                Assert.AreEqual("Existing John", user.Data.FullName);
                Assert.AreEqual("user1", user.Data.LoginName);
                Assert.NotNull(user.Data.Groups);
                Assert.That(user.Data.Groups.Any(v => v == "admin"));
                Assert.That(user.Data.Groups.Any(v => v == "other"));
                Assert.AreEqual(false, user.Data.Disabled);
            }

            [Test]
            public void does_not_override_user_password()
            {
                _consumer.HandledMessages.Clear();
                _users.Handle(new UserManagementMessage.ChangePassword(_replyTo, "user1", "existing!", "new-password"));
                var updateResult = _consumer.HandledMessages.OfType<UserManagementMessage.UpdateResult>().Last();
                Assert.NotNull(updateResult);
                Assert.IsTrue(updateResult.Success);
            }
        }

        [TestFixture]
        public class when_updating_user_details : TestFixtureWithUserManagementService
        {
            protected override void GivenCommands()
            {
                base.GivenCommands();
                _users.Handle(
                    new UserManagementMessage.Create(
                        _replyTo, "user1", "John Doe", new[] {"admin", "other"}, "Johny123!"));
            }

            protected override void When()
            {
                base.When();
                _users.Handle(new UserManagementMessage.Update(_replyTo, "user1", "Doe John", new[] {"good"}));
            }

            [Test]
            public void replies_success()
            {
                var updateResults = _consumer.HandledMessages.OfType<UserManagementMessage.UpdateResult>().ToList();
                Assert.AreEqual(1, updateResults.Count);
                Assert.IsTrue(updateResults[0].Success);
            }

            [Test]
            public void reply_has_the_correct_login_name()
            {
                var updateResults = _consumer.HandledMessages.OfType<UserManagementMessage.UpdateResult>().ToList();
                Assert.AreEqual(1, updateResults.Count);
                Assert.AreEqual("user1", updateResults[0].LoginName);
            }

            [Test]
            public void updates_details()
            {
                _users.Handle(new UserManagementMessage.Get(_replyTo, "user1"));
                var user = _consumer.HandledMessages.OfType<UserManagementMessage.UserDetailsResult>().SingleOrDefault();
                Assert.NotNull(user);
                Assert.AreEqual("Doe John", user.Data.FullName);
                Assert.AreEqual("user1", user.Data.LoginName);
                Assert.NotNull(user.Data.Groups);
                Assert.That(user.Data.Groups.All(v => v != "admin"));
                Assert.That(user.Data.Groups.All(v => v != "other"));
                Assert.That(user.Data.Groups.Any(v => v == "good"));
                Assert.AreEqual(false, user.Data.Disabled);
            }

            [Test]
            public void does_not_update_enabled()
            {
                _users.Handle(new UserManagementMessage.Get(_replyTo, "user1"));
                var user = _consumer.HandledMessages.OfType<UserManagementMessage.UserDetailsResult>().SingleOrDefault();
                Assert.NotNull(user);
                Assert.AreEqual(false, user.Data.Disabled);
            }

            [Test]
            public void does_not_change_password()
            {
                _consumer.HandledMessages.Clear();
                _users.Handle(new UserManagementMessage.ChangePassword(_replyTo, "user1", "Johny123!", "new-password"));
                var updateResult = _consumer.HandledMessages.OfType<UserManagementMessage.UpdateResult>().Last();
                Assert.NotNull(updateResult);
                Assert.IsTrue(updateResult.Success);
            }
        }

        [TestFixture]
        public class when_updating_non_existing_user_details : TestFixtureWithUserManagementService
        {
            protected override void GivenCommands()
            {
                base.GivenCommands();
            }

            protected override void When()
            {
                base.When();
                _users.Handle(new UserManagementMessage.Update(_replyTo, "user1", "Doe John", new[] {"admin", "other"}));
            }

            [Test]
            public void replies_not_found()
            {
                var updateResults = _consumer.HandledMessages.OfType<UserManagementMessage.UpdateResult>().ToList();
                Assert.AreEqual(1, updateResults.Count);
                Assert.IsFalse(updateResults[0].Success);
                Assert.AreEqual(UserManagementMessage.Error.NotFound, updateResults[0].Error);
            }

            [Test]
            public void reply_has_the_correct_login_name()
            {
                var updateResults = _consumer.HandledMessages.OfType<UserManagementMessage.UpdateResult>().ToList();
                Assert.AreEqual(1, updateResults.Count);
                Assert.AreEqual("user1", updateResults[0].LoginName);
            }

            [Test]
            public void does_not_create_a_user_account()
            {
                _users.Handle(new UserManagementMessage.Get(_replyTo, "user1"));
                var user = _consumer.HandledMessages.OfType<UserManagementMessage.UserDetailsResult>().SingleOrDefault();
                Assert.NotNull(user);
                Assert.IsFalse(user.Success);
                Assert.AreEqual(UserManagementMessage.Error.NotFound, user.Error);
                Assert.Null(user.Data);
            }
        }

        [TestFixture]
        public class when_updating_a_disabled_user_account_details : TestFixtureWithUserManagementService
        {
            protected override void GivenCommands()
            {
                base.GivenCommands();
                _users.Handle(
                    new UserManagementMessage.Create(
                        _replyTo, "user1", "John Doe", new[] {"admin", "other"}, "Johny123!"));
                _users.Handle(new UserManagementMessage.Disable(_replyTo, "user1"));
            }

            protected override void When()
            {
                base.When();
                _users.Handle(new UserManagementMessage.Update(_replyTo, "user1", "Doe John", new[] {"good"}));
            }

            [Test]
            public void replies_success()
            {
                var updateResults = _consumer.HandledMessages.OfType<UserManagementMessage.UpdateResult>().ToList();
                Assert.AreEqual(1, updateResults.Count);
                Assert.IsTrue(updateResults[0].Success);
            }

            [Test]
            public void does_not_update_enabled()
            {
                _users.Handle(new UserManagementMessage.Get(_replyTo, "user1"));
                var user = _consumer.HandledMessages.OfType<UserManagementMessage.UserDetailsResult>().SingleOrDefault();
                Assert.NotNull(user);
                Assert.AreEqual(true, user.Data.Disabled);
            }
        }

        [TestFixture]
        public class when_disabling_an_enabled_user_account : TestFixtureWithUserManagementService
        {
            protected override void GivenCommands()
            {
                base.GivenCommands();
                _users.Handle(
                    new UserManagementMessage.Create(
                        _replyTo, "user1", "John Doe", new[] {"admin", "other"}, "Johny123!"));
            }

            protected override void When()
            {
                base.When();
                _users.Handle(new UserManagementMessage.Disable(_replyTo, "user1"));
            }

            [Test]
            public void replies_success_with_correct_login_name_set()
            {
                var updateResults = _consumer.HandledMessages.OfType<UserManagementMessage.UpdateResult>().ToList();
                Assert.AreEqual(1, updateResults.Count);
                Assert.IsTrue(updateResults[0].Success);
                Assert.AreEqual("user1", updateResults[0].LoginName);
            }

            [Test]
            public void disables_user_account()
            {
                _users.Handle(new UserManagementMessage.Get(_replyTo, "user1"));
                var user = _consumer.HandledMessages.OfType<UserManagementMessage.UserDetailsResult>().SingleOrDefault();
                Assert.NotNull(user);
                Assert.AreEqual(true, user.Data.Disabled);
            }
        }

        [TestFixture]
        public class when_disabling_a_disabled_user_account : TestFixtureWithUserManagementService
        {
            protected override void GivenCommands()
            {
                base.GivenCommands();
                _users.Handle(
                    new UserManagementMessage.Create(
                        _replyTo, "user1", "John Doe", new[] {"admin", "other"}, "Johny123!"));
                _users.Handle(new UserManagementMessage.Disable(_replyTo, "user1"));
            }

            protected override void When()
            {
                base.When();
                _users.Handle(new UserManagementMessage.Disable(_replyTo, "user1"));
            }

            [Test]
            public void replies_success_with_correct_login_name_set()
            {
                var updateResults = _consumer.HandledMessages.OfType<UserManagementMessage.UpdateResult>().ToList();
                Assert.AreEqual(1, updateResults.Count);
                Assert.IsTrue(updateResults[0].Success);
                Assert.AreEqual("user1", updateResults[0].LoginName);
            }

            [Test]
            public void keeps_disabled()
            {
                _users.Handle(new UserManagementMessage.Get(_replyTo, "user1"));
                var user = _consumer.HandledMessages.OfType<UserManagementMessage.UserDetailsResult>().SingleOrDefault();
                Assert.NotNull(user);
                Assert.AreEqual(true, user.Data.Disabled);
            }
        }

        [TestFixture]
        public class when_enabling_a_disabled_user_account : TestFixtureWithUserManagementService
        {
            protected override void GivenCommands()
            {
                base.GivenCommands();
                _users.Handle(
                    new UserManagementMessage.Create(
                        _replyTo, "user1", "John Doe", new[] {"admin", "other"}, "Johny123!"));
                _users.Handle(new UserManagementMessage.Disable(_replyTo, "user1"));
            }

            protected override void When()
            {
                base.When();
                _users.Handle(new UserManagementMessage.Enable(_replyTo, "user1"));
            }

            [Test]
            public void replies_success_with_correct_login_name_set()
            {
                var updateResults = _consumer.HandledMessages.OfType<UserManagementMessage.UpdateResult>().ToList();
                Assert.AreEqual(1, updateResults.Count);
                Assert.IsTrue(updateResults[0].Success);
                Assert.AreEqual("user1", updateResults[0].LoginName);
            }

            [Test]
            public void enables_user_account()
            {
                _users.Handle(new UserManagementMessage.Get(_replyTo, "user1"));
                var user = _consumer.HandledMessages.OfType<UserManagementMessage.UserDetailsResult>().SingleOrDefault();
                Assert.NotNull(user);
                Assert.AreEqual(false, user.Data.Disabled);
            }
        }

        [TestFixture]
        public class when_enabling_an_enabled_user_account : TestFixtureWithUserManagementService
        {
            protected override void GivenCommands()
            {
                base.GivenCommands();
                _users.Handle(
                    new UserManagementMessage.Create(
                        _replyTo, "user1", "John Doe", new[] {"admin", "other"}, "Johny123!"));
            }

            protected override void When()
            {
                base.When();
                _users.Handle(new UserManagementMessage.Enable(_replyTo, "user1"));
            }

            [Test]
            public void replies_success_with_correct_login_name_set()
            {
                var updateResults = _consumer.HandledMessages.OfType<UserManagementMessage.UpdateResult>().ToList();
                Assert.AreEqual(1, updateResults.Count);
                Assert.IsTrue(updateResults[0].Success);
                Assert.AreEqual("user1", updateResults[0].LoginName);
            }

            [Test]
            public void keeps_enabled()
            {
                _users.Handle(new UserManagementMessage.Get(_replyTo, "user1"));
                var user = _consumer.HandledMessages.OfType<UserManagementMessage.UserDetailsResult>().SingleOrDefault();
                Assert.NotNull(user);
                Assert.AreEqual(false, user.Data.Disabled);
            }
        }

        [TestFixture]
        public class when_resetting_the_password : TestFixtureWithUserManagementService
        {
            protected override void GivenCommands()
            {
                base.GivenCommands();
                _users.Handle(
                    new UserManagementMessage.Create(
                        _replyTo, "user1", "John Doe", new[] {"admin", "other"}, "Johny123!"));
            }

            protected override void When()
            {
                base.When();
                _users.Handle(new UserManagementMessage.ResetPassword(_replyTo, "user1", "new-password"));
            }

            [Test]
            public void replies_success()
            {
                var updateResults = _consumer.HandledMessages.OfType<UserManagementMessage.UpdateResult>().ToList();
                Assert.AreEqual(1, updateResults.Count);
                Assert.IsTrue(updateResults[0].Success);
            }

            [Test]
            public void reply_has_the_correct_login_name()
            {
                var updateResults = _consumer.HandledMessages.OfType<UserManagementMessage.UpdateResult>().ToList();
                Assert.AreEqual(1, updateResults.Count);
                Assert.AreEqual("user1", updateResults[0].LoginName);
            }

            [Test]
            public void does_not_update_details()
            {
                _users.Handle(new UserManagementMessage.Get(_replyTo, "user1"));
                var user = _consumer.HandledMessages.OfType<UserManagementMessage.UserDetailsResult>().SingleOrDefault();
                Assert.NotNull(user);
                Assert.AreEqual("John Doe", user.Data.FullName);
                Assert.AreEqual("user1", user.Data.LoginName);
                Assert.AreEqual(false, user.Data.Disabled);
            }

            [Test]
            public void changes_password()
            {
                _consumer.HandledMessages.Clear();
                _users.Handle(
                    new UserManagementMessage.ChangePassword(_replyTo, "user1", "new-password", "other-password"));
                var updateResult = _consumer.HandledMessages.OfType<UserManagementMessage.UpdateResult>().Last();
                Assert.NotNull(updateResult);
                Assert.IsTrue(updateResult.Success);
            }
        }

        [TestFixture]
        public class when_changing_a_password_with_correct_current_password : TestFixtureWithUserManagementService
        {
            protected override void GivenCommands()
            {
                base.GivenCommands();
                _users.Handle(
                    new UserManagementMessage.Create(
                        _replyTo, "user1", "John Doe", new[] {"admin", "other"}, "Johny123!"));
            }

            protected override void When()
            {
                base.When();
                _users.Handle(new UserManagementMessage.ChangePassword(_replyTo, "user1", "Johny123!", "new-password"));
            }

            [Test]
            public void replies_success()
            {
                var updateResults = _consumer.HandledMessages.OfType<UserManagementMessage.UpdateResult>().ToList();
                Assert.AreEqual(1, updateResults.Count);
                Assert.IsTrue(updateResults[0].Success);
            }

            [Test]
            public void reply_has_the_correct_login_name()
            {
                var updateResults = _consumer.HandledMessages.OfType<UserManagementMessage.UpdateResult>().ToList();
                Assert.AreEqual(1, updateResults.Count);
                Assert.AreEqual("user1", updateResults[0].LoginName);
            }

            [Test]
            public void does_not_update_details()
            {
                _users.Handle(new UserManagementMessage.Get(_replyTo, "user1"));
                var user = _consumer.HandledMessages.OfType<UserManagementMessage.UserDetailsResult>().SingleOrDefault();
                Assert.NotNull(user);
                Assert.AreEqual("John Doe", user.Data.FullName);
                Assert.AreEqual("user1", user.Data.LoginName);
                Assert.AreEqual(false, user.Data.Disabled);
            }

            [Test]
            public void changes_password()
            {
                _consumer.HandledMessages.Clear();
                _users.Handle(
                    new UserManagementMessage.ChangePassword(_replyTo, "user1", "new-password", "other-password"));
                var updateResult = _consumer.HandledMessages.OfType<UserManagementMessage.UpdateResult>().Last();
                Assert.NotNull(updateResult);
                Assert.IsTrue(updateResult.Success);
            }
        }

        [TestFixture]
        public class when_changing_a_password_with_incorrect_current_password : TestFixtureWithUserManagementService
        {
            protected override void GivenCommands()
            {
                base.GivenCommands();
                _users.Handle(
                    new UserManagementMessage.Create(
                        _replyTo, "user1", "John Doe", new[] {"admin", "other"}, "Johny123!"));
            }

            protected override void When()
            {
                base.When();
                _users.Handle(new UserManagementMessage.ChangePassword(_replyTo, "user1", "incorrect", "new-password"));
            }

            [Test]
            public void replies_unauthorized()
            {
                var updateResults = _consumer.HandledMessages.OfType<UserManagementMessage.UpdateResult>().ToList();
                Assert.AreEqual(1, updateResults.Count);
                Assert.IsFalse(updateResults[0].Success);
                Assert.AreEqual(UserManagementMessage.Error.Unauthorized, updateResults[0].Error);
            }

            [Test]
            public void reply_has_the_correct_login_name()
            {
                var updateResults = _consumer.HandledMessages.OfType<UserManagementMessage.UpdateResult>().ToList();
                Assert.AreEqual(1, updateResults.Count);
                Assert.AreEqual("user1", updateResults[0].LoginName);
            }

            [Test]
            public void does_not_update_details()
            {
                _users.Handle(new UserManagementMessage.Get(_replyTo, "user1"));
                var user = _consumer.HandledMessages.OfType<UserManagementMessage.UserDetailsResult>().SingleOrDefault();
                Assert.NotNull(user);
                Assert.AreEqual("John Doe", user.Data.FullName);
                Assert.AreEqual("user1", user.Data.LoginName);
                Assert.AreEqual(false, user.Data.Disabled);
            }

            [Test]
            public void does_not_change_the_password()
            {
                _consumer.HandledMessages.Clear();
                _users.Handle(
                    new UserManagementMessage.ChangePassword(_replyTo, "user1", "Johny123!", "other-password"));
                var updateResult = _consumer.HandledMessages.OfType<UserManagementMessage.UpdateResult>().Last();
                Assert.NotNull(updateResult);
                Assert.IsTrue(updateResult.Success);
            }
        }

        [TestFixture]
        public class when_deleting_an_existing_user_account : TestFixtureWithUserManagementService
        {
            protected override void GivenCommands()
            {
                base.GivenCommands();
                _users.Handle(
                    new UserManagementMessage.Create(
                        _replyTo, "user1", "John Doe", new[] {"admin", "other"}, "Johny123!"));
            }

            protected override void When()
            {
                base.When();
                _users.Handle(new UserManagementMessage.Delete(_replyTo, "user1"));
            }

            [Test]
            public void replies_success()
            {
                var updateResults = _consumer.HandledMessages.OfType<UserManagementMessage.UpdateResult>().ToList();
                Assert.AreEqual(1, updateResults.Count);
                Assert.IsTrue(updateResults[0].Success);
            }

            [Test]
            public void reply_has_the_correct_login_name()
            {
                var updateResults = _consumer.HandledMessages.OfType<UserManagementMessage.UpdateResult>().ToList();
                Assert.AreEqual(1, updateResults.Count);
                Assert.AreEqual("user1", updateResults[0].LoginName);
            }

            [Test]
            public void deletes_the_user_account()
            {
                _users.Handle(new UserManagementMessage.Get(_replyTo, "user1"));
                var user = _consumer.HandledMessages.OfType<UserManagementMessage.UserDetailsResult>().SingleOrDefault();
                Assert.NotNull(user);
                Assert.IsFalse(user.Success);
                Assert.AreEqual(UserManagementMessage.Error.NotFound, user.Error);
                Assert.Null(user.Data);
            }
        }

        [TestFixture]
        public class when_getting_all_users : TestFixtureWithUserManagementService
        {
            protected override void Given()
            {
                base.Given();
                // simulate index-users projection
                ExistingEvent("$users", "$User", null, "user1");
                ExistingEvent("$users", "$User", null, "user2");
                ExistingEvent("$users", "$User", null, "user3");
                ExistingEvent("$users", "$User", null, "another_user");
            }

            protected override void GivenCommands()
            {
                base.GivenCommands();
                _users.Handle(
                    new UserManagementMessage.Create(
                        _replyTo, "user1", "John Doe 1", new[] {"admin1", "other"}, "Johny123!"));
                _users.Handle(
                    new UserManagementMessage.Create(
                        _replyTo, "user2", "John Doe 2", new[] {"admin2", "other"}, "Johny123!"));
                _users.Handle(
                    new UserManagementMessage.Create(
                        _replyTo, "user3", "Another Doe 1", new[] {"admin3", "other"}, "Johny123!"));
                _users.Handle(
                    new UserManagementMessage.Create(
                        _replyTo, "another_user", "Another Doe 2", new[] {"admin4", "other"}, "Johny123!"));
            }

            protected override void When()
            {
                base.When();
                _users.Handle(new UserManagementMessage.GetAll(_replyTo));
            }

            [Test]
            public void returns_all_user_accounts()
            {
                var users = _consumer.HandledMessages.OfType<UserManagementMessage.AllUserDetailsResult>().Single().Data;

                Assert.AreEqual(4, users.Length);
            }

            [Test]
            public void returns_in_the_login_name_order()
            {
                var users = _consumer.HandledMessages.OfType<UserManagementMessage.AllUserDetailsResult>().Single().Data;

                Assert.That(
                    new[] {"another_user", "user1", "user2", "user3"}.SequenceEqual(users.Select(v => v.LoginName)));
            }

            [Test]
            public void returns_full_names()
            {
                var users = _consumer.HandledMessages.OfType<UserManagementMessage.AllUserDetailsResult>().Single().Data;

                Assert.That(users.Any(v => v.LoginName == "user2" && v.FullName == "John Doe 2"));
            }
        }
    }
}