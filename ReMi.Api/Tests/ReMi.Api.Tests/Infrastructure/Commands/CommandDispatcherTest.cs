using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Http.Dependencies;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.Api.Insfrastructure.Commands;
using ReMi.Api.Insfrastructure.Security;
using ReMi.BusinessEntities.Auth;
using ReMi.Common.Utils;
using ReMi.Common.WebApi.Tracking;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.TestUtils.UnitTests;

namespace ReMi.Api.Tests.Infrastructure.Commands
{
    public class CommandDispatcherTest : TestClassFor<CommandDispatcher>
    {
        private Mock<IDependencyResolver> _dependencyResolverMock;
        private Mock<IHandleCommand<TestCommand>> _handleCommandMock;
        private Mock<ICommandTracker> _commandTrackerMock;
        private Mock<ISerialization> _serializationMock;
        private Mock<IApplicationSettings> _applicationSettingsMock;

        protected override CommandDispatcher ConstructSystemUnderTest()
        {
            return new CommandDispatcher
            {
                DependencyResolver = _dependencyResolverMock.Object,
                CommandTracker = _commandTrackerMock.Object,
                Serialization = _serializationMock.Object,
                ApplicationSettings = _applicationSettingsMock.Object
            };
        }


        protected override void TestInitialize()
        {
            _handleCommandMock = new Mock<IHandleCommand<TestCommand>>();
            _commandTrackerMock = new Mock<ICommandTracker>();
            _serializationMock = new Mock<ISerialization>(MockBehavior.Strict);
            _applicationSettingsMock = new Mock<IApplicationSettings>();
            _applicationSettingsMock.SetupGet(x => x.LogJsonFormatted).Returns(true);

            _dependencyResolverMock = new Mock<IDependencyResolver>();
            _dependencyResolverMock.Setup(o => o.GetService(typeof(IHandleCommand<TestCommand>)))
                .Returns(_handleCommandMock.Object);

            base.TestInitialize();

        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void Send_ShouldRaiseException_WhenCantPopulateContext()
        {
            Thread.CurrentPrincipal = null;

            var command = new TestCommand();

            var task = Sut.Send(command);
            task.Wait();

            _handleCommandMock.Verify(x => x.Handle(command));
        }

        [Test]
        public void Send_ShouldPopulateCommandContext_WhenCommandIsSent()
        {
            var account = Builder<Account>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.FullName, RandomData.RandomString(20))
                .With(o => o.Role, Builder<Role>.CreateNew()
                    .With(r => r.Name, RandomData.RandomString(10))
                    .Build())
                .Build();
            Thread.CurrentPrincipal = new RequestPrincipal(account);

            var command = new TestCommand();
            _serializationMock.Setup(x => x.ToJson(command, It.Is<IEnumerable<string>>(s => s.First() == "Password"), It.IsAny<bool>()))
                .Returns("json data");

            var task = Sut.Send(command);
            task.Wait();

            _handleCommandMock.Verify(
                x => x.Handle(It.Is<TestCommand>(o =>
                    o.CommandContext != null
                    && o.CommandContext.UserId == account.ExternalId
                    && o.CommandContext.UserName == account.FullName
                    && o.CommandContext.UserRole == account.Role.Name
                )));
        }

        [Test]
        public void Send_ShouldRunProperCommandHandler_WhenCommandIsSent()
        {
            var command = new TestCommand { CommandContext = new CommandContext { Id = Guid.NewGuid(), UserName = "remi"  } };
            var account = Builder<Account>.CreateNew().Build();
            Thread.CurrentPrincipal = new RequestPrincipal(account);
            _serializationMock.Setup(x => x.ToJson(command, It.Is<IEnumerable<string>>(s => s.First() == "Password"), It.IsAny<bool>()))
                .Returns("json data");


            var task = Sut.Send(command);
            task.Wait();

            _handleCommandMock.Verify(x => x.Handle(command));
            _commandTrackerMock.Verify(x => x.CreateTracker(command.CommandContext.Id, "TestCommand"));
            _commandTrackerMock.Verify(x => x.Started(command.CommandContext.Id));
            _commandTrackerMock.Verify(x => x.Finished(command.CommandContext.Id, null));
            _serializationMock.Verify(x => x.ToJson(It.IsAny<object>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>()), Times.Once);
        }

        [Test]
        public void Send_ShouldCreateTracker_BeforeHandleCommand()
        {
            var command = new TestCommand { CommandContext = new CommandContext { Id = Guid.NewGuid(), UserName = "remi" } };
            var account = Builder<Account>.CreateNew().Build();
            Thread.CurrentPrincipal = new RequestPrincipal(account);

            var callOrder = 0;
            var createTrackerOrder = -1;
            var handleCommandOrder = -1;
            _handleCommandMock.Setup(x => x.Handle(command)).Callback(() => handleCommandOrder = callOrder++);
            _commandTrackerMock.Setup(x => x.CreateTracker(command.CommandContext.Id, "TestCommand")).Callback(() => createTrackerOrder = callOrder++);
            _serializationMock.Setup(x => x.ToJson(command, It.Is<IEnumerable<string>>(s => s.First() == "Password"), It.IsAny<bool>()))
                .Returns("json data");

            var task = Sut.Send(command);
            task.Wait();

            Assert.AreEqual(createTrackerOrder, 0);
            Assert.AreEqual(handleCommandOrder, 1);
        }


        [Test]
        public void Send_ShouldRunProperCommandHandler_WhenCommandIsSync()
        {
            var command = new TestCommand { CommandContext = new CommandContext { Id = Guid.NewGuid(), UserName = "remi", IsSynchronous = true } };
            var account = Builder<Account>.CreateNew().Build();
            Thread.CurrentPrincipal = new RequestPrincipal(account);
            _serializationMock.Setup(x => x.ToJson(command, It.Is<IEnumerable<string>>(s => s.First() == "Password"), It.IsAny<bool>()))
                .Returns("json data");


            var task = Sut.Send(command);
            Assert.IsNull(task);

            _handleCommandMock.Verify(x => x.Handle(command));
            _commandTrackerMock.Verify(x => x.CreateTracker(command.CommandContext.Id, "TestCommand"));
            _commandTrackerMock.Verify(x => x.Started(command.CommandContext.Id));
            _commandTrackerMock.Verify(x => x.Finished(command.CommandContext.Id, null));
        }


        [Test]
        public void Send_ShouldHandleCommandInDifferentThread_WhenCommandIsAsync()
        {
            var command = new TestCommand { CommandContext = new CommandContext { Id = Guid.NewGuid(), UserName = "remi" } };
            var account = Builder<Account>.CreateNew().Build();
            Thread.CurrentPrincipal = new RequestPrincipal(account);

            var handleThreadId = -1;
            _handleCommandMock.Setup(x => x.Handle(command)).Callback(() => handleThreadId = Thread.CurrentThread.ManagedThreadId);
            _serializationMock.Setup(x => x.ToJson(command, It.Is<IEnumerable<string>>(s => s.First() == "Password"), It.IsAny<bool>()))
                .Returns("json data");

            var task = Sut.Send(command);
            task.Wait();

            Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, handleThreadId);
        }

        [Test]
        public void Send_ShouldHandleCommandInDifferentThread_WhenCommandIsSync()
        {
            var command = new TestCommand { CommandContext = new CommandContext { Id = Guid.NewGuid(), UserName = "remi", IsSynchronous = true } };
            var account = Builder<Account>.CreateNew().Build();
            Thread.CurrentPrincipal = new RequestPrincipal(account);

            var handleThreadId = -1;
            _handleCommandMock.Setup(x => x.Handle(command)).Callback(() => handleThreadId = Thread.CurrentThread.ManagedThreadId);
            _serializationMock.Setup(x => x.ToJson(command, It.Is<IEnumerable<string>>(s => s.First() == "Password"), It.IsAny<bool>()))
                .Returns("json data");

            Sut.Send(command);

            Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, handleThreadId);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Send_ShouldThrowArgumentNullException_WhenNullCommandIsSent()
        {
            Sut.Send((TestCommand)null);
        }


        public class TestCommand : ICommand
        {
            public CommandContext CommandContext { get; set; }
        }

        public class CommandHandlerTest<T> : IHandleCommand<T> where T : ICommand
        {
            private readonly IHandleCommand<TestCommand> _injectedImplementation;

            public CommandHandlerTest(IHandleCommand<TestCommand> injectedImplementation)
            {
                _injectedImplementation = injectedImplementation;
            }


            public void Handle(T command)
            {
                _injectedImplementation.Handle(command as TestCommand);
            }
        }
    }
}
