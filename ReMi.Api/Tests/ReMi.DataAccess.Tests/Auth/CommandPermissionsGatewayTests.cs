using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Api;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.Common.Utils.Repository;
using ReMi.Contracts.Plugins.Data;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.Auth;
using ReMi.DataEntities.Plugins;
using DataCommand = ReMi.DataEntities.Api.Command;
using DataCommandPermission = ReMi.DataEntities.Auth.CommandPermission;
using DataRole = ReMi.DataEntities.Auth.Role;

namespace ReMi.DataAccess.Tests.Auth
{
    [TestFixture]
    public class CommandPermissionsGatewayTests : TestClassFor<CommandPermissionsGateway>
    {
        private Mock<IRepository<DataCommand>> _commandRepositoryMock;
        private Mock<IRepository<DataCommandPermission>> _commandPermissionRepositoryMock;
        private Mock<IRepository<DataRole>> _roleRepositoryMock;
        private Mock<IRepository<Account>> _accountRepositoryMock;
        private Mock<IRepository<PluginConfiguration>> _pluginConfigurationRepositoryMock;
        private Mock<IMappingEngine> _mapperMock;

        protected override CommandPermissionsGateway ConstructSystemUnderTest()
        {
            return new CommandPermissionsGateway
            {
                CommandRepository = _commandRepositoryMock.Object,
                CommandPermissionRepository = _commandPermissionRepositoryMock.Object,
                RoleRepository = _roleRepositoryMock.Object,
                AccountRepository = _accountRepositoryMock.Object,
                PluginConfigurationRepository = _pluginConfigurationRepositoryMock.Object,
                Mapper = _mapperMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _commandRepositoryMock = new Mock<IRepository<DataCommand>>(MockBehavior.Strict);
            _commandPermissionRepositoryMock = new Mock<IRepository<DataCommandPermission>>(MockBehavior.Strict);
            _roleRepositoryMock = new Mock<IRepository<DataRole>>(MockBehavior.Strict);
            _accountRepositoryMock = new Mock<IRepository<Account>>(MockBehavior.Strict);
            _pluginConfigurationRepositoryMock = new Mock<IRepository<PluginConfiguration>>(MockBehavior.Strict);
            _mapperMock = new Mock<IMappingEngine>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Dispose_ShouldDisposeAllRepositories_WhenInvoked()
        {
            _commandRepositoryMock.Setup(x => x.Dispose());
            _commandPermissionRepositoryMock.Setup(x => x.Dispose());
            _roleRepositoryMock.Setup(x => x.Dispose());
            _accountRepositoryMock.Setup(x => x.Dispose());
            _pluginConfigurationRepositoryMock.Setup(x => x.Dispose());

            Sut.Dispose();

            _commandRepositoryMock.Verify(x => x.Dispose(), Times.Once);
            _commandPermissionRepositoryMock.Verify(x => x.Dispose(), Times.Once);
            _roleRepositoryMock.Verify(x => x.Dispose(), Times.Once);
            _accountRepositoryMock.Verify(x => x.Dispose(), Times.Once);
            _pluginConfigurationRepositoryMock.Verify(x => x.Dispose(), Times.Once);
        }

        [Test]
        public void GetCommands_ShouldReturnAllNotBackgroundCommands_WhenCalledWithoutParameter()
        {
            var commands = BuildCommandEntities();
            var notBackgroundCount = commands.Count(x => !x.IsBackground);

            _commandRepositoryMock.SetupEntities(commands);
            _mapperMock.Setup(x => x.Map<DataCommand, Command>(It.IsAny<DataCommand>()))
                .Returns(new Command());


            var result = Sut.GetCommands();


            Assert.AreEqual(notBackgroundCount, result.Count());
            _mapperMock.Verify(x => x.Map<DataCommand, Command>(It.IsAny<DataCommand>()), Times.Exactly(notBackgroundCount));
        }

        [Test]
        public void GetCommands_ShouldReturnAllCommands_WhenCalledWithoutTrueParameter()
        {
            var commands = BuildCommandEntities();
            var count = commands.Count();

            _commandRepositoryMock.SetupEntities(commands);
            _mapperMock.Setup(x => x.Map<DataCommand, Command>(It.IsAny<DataCommand>()))
                .Returns(new Command());


            var result = Sut.GetCommands(true);


            Assert.AreEqual(count, result.Count());
            _mapperMock.Verify(x => x.Map<DataCommand, Command>(It.IsAny<DataCommand>()), Times.Exactly(count));
        }

        [Test]
        public void AddCommandPermission_ShouldAddNewCommandToRoleRelation_WhenCommandAndRoleExist()
        {
            var roles = BuildRoleEntities();
            var commands = BuildCommandEntities();
            var startPermissionCount = commands.First().CommandPermissions.Count;

            _commandRepositoryMock.SetupEntities(commands);
            _roleRepositoryMock.SetupEntities(roles);
            _commandRepositoryMock.Setup(x => x.Update(commands.First()))
                .Returns((ChangedFields<DataCommand>)null);

            Sut.AddCommandPermission(commands.First().CommandId, roles.First().ExternalId);

            Assert.IsTrue(commands.First().CommandPermissions.Any(x => x.RoleId == roles.First().Id));
            Assert.AreEqual(startPermissionCount + 1, commands.First().CommandPermissions.Count);
            _commandRepositoryMock.Verify(x => x.Update(commands.First()), Times.Once());
        }

        [Test]
        [ExpectedException(typeof(CommandNotFoundException))]
        public void AddCommandPermission_ShouldThrowException_WhenCommandNotExists()
        {
            var roles = BuildRoleEntities();
            var commands = BuildCommandEntities();

            _commandRepositoryMock.SetupEntities(commands);

            Sut.AddCommandPermission(-1, roles.First().ExternalId);
        }

        [Test]
        [ExpectedException(typeof(RoleNotFoundException))]
        public void AddCommandPermission_ShouldThrowException_WhenRoleNotExists()
        {
            var roles = BuildRoleEntities();
            var commands = BuildCommandEntities();

            _commandRepositoryMock.SetupEntities(commands);
            _roleRepositoryMock.SetupEntities(roles);

            Sut.AddCommandPermission(commands.First().CommandId, Guid.Empty);
        }

        [Test]
        public void AddCommandPermission_ShouldDoNothing_WhenPermissionExists()
        {
            var roles = BuildRoleEntities();
            var commands = BuildCommandEntities();
            commands.First().CommandPermissions.Add(new DataCommandPermission
            {
                CommandId = commands.First().CommandId,
                RoleId = roles.First().Id,
                Role = roles.First()
            });
            var startPermissionCount = commands.First().CommandPermissions.Count;

            _commandRepositoryMock.SetupEntities(commands);
            _roleRepositoryMock.SetupEntities(roles);

            Sut.AddCommandPermission(commands.First().CommandId, roles.First().ExternalId);

            Assert.AreEqual(startPermissionCount, commands.First().CommandPermissions.Count);
            _commandRepositoryMock.Verify(x => x.Update(It.IsAny<DataCommand>()), Times.Never);
        }

        [Test]
        public void RemoveCommandPermission_ShouldRemoveCommandToRoleRelation_WhenCommandAndRoleExist()
        {
            var roles = BuildRoleEntities();
            var commands = BuildCommandEntities();
            commands.First().CommandPermissions.Add(new DataCommandPermission
            {
                CommandId = commands.First().CommandId,
                RoleId = roles.First().Id,
                Role = roles.First()
            });

            _commandRepositoryMock.SetupEntities(commands);
            _roleRepositoryMock.SetupEntities(roles);
            _commandPermissionRepositoryMock.SetupEntities(commands.First().CommandPermissions);
            _commandPermissionRepositoryMock.Setup(x =>x.Delete(It.Is<DataCommandPermission>(
                r => r.CommandId == commands.First().CommandId && r.RoleId == roles.First().Id)));

            Sut.RemoveCommandPermission(commands.First().CommandId, roles.First().ExternalId);

            Assert.IsTrue(commands.First().CommandPermissions.Any(x => x.RoleId == roles.First().Id));
            _commandPermissionRepositoryMock.Verify(x => x.Delete(It.Is<DataCommandPermission>(
                r => r.CommandId == commands.First().CommandId && r.RoleId == roles.First().Id)), Times.Once());

        }

        [Test]
        [ExpectedException(typeof(CommandNotFoundException))]
        public void RemoveCommandPermission_ShouldThrowException_WhenCommandNotExists()
        {
            var roles = BuildRoleEntities();
            var commands = BuildCommandEntities();

            _commandRepositoryMock.SetupEntities(commands);

            Sut.RemoveCommandPermission(-1, roles.First().ExternalId);
        }

        [Test]
        [ExpectedException(typeof(RoleNotFoundException))]
        public void RemoveCommandPermission_ShouldThrowException_WhenRoleNotExists()
        {
            var roles = BuildRoleEntities();
            var commands = BuildCommandEntities();

            _commandRepositoryMock.SetupEntities(commands);
            _roleRepositoryMock.SetupEntities(roles);

            Sut.RemoveCommandPermission(commands.First().CommandId, Guid.Empty);
        }

        [Test]
        public void RemoveCommandPermission_ShouldDoNothing_WhenPermissionExists()
        {
            var roles = BuildRoleEntities();
            var commands = BuildCommandEntities();
            var startPermissionCount = commands.First().CommandPermissions.Count;

            _commandRepositoryMock.SetupEntities(commands);
            _roleRepositoryMock.SetupEntities(roles);

            Sut.RemoveCommandPermission(commands.First().CommandId, roles.First().ExternalId);

            _commandPermissionRepositoryMock.Verify(x => x.Delete(It.IsAny<DataCommandPermission>()), Times.Never);
        }

        [Test]
        public void GetAllowedCommands_ShouldReturnCorrectValue()
        {
            var commandPermissions = new List<DataCommandPermission>
             {
                 new DataCommandPermission
                 {
                     Command = new DataCommand{Name = RandomData.RandomString(15,20)},
                     Role = new DataRole
                     {
                         ExternalId = Guid.NewGuid()
                     }
                 }
             };

            _roleRepositoryMock.SetupEntities(commandPermissions.Select(x => x.Role).ToArray());
            _commandPermissionRepositoryMock.SetupEntities(commandPermissions);

            var result = Sut.GetAllowedCommands(commandPermissions[0].Role.ExternalId);

            Assert.AreEqual(commandPermissions[0].Command.Name, result.First());
        }

        [Test]
        public void GetAllowedCommands_ShouldReturnEmptyCollection_WhenRoleIsEmptyAndAuthenticationMethodDefined()
        {
            var commands = Builder<DataCommand>.CreateListOfSize(5).Build();
            var accounts = Builder<Account>.CreateListOfSize(5).Build();
            var pluginsConfig = Builder<PluginConfiguration>.CreateListOfSize(5)
                .Random(1)
                .With(x => x.PluginType, PluginType.Authentication)
                .With(x => x.PluginId, RandomData.RandomInt(int.MaxValue))
                .Build();

            _accountRepositoryMock.SetupEntities(accounts);
            _pluginConfigurationRepositoryMock.SetupEntities(pluginsConfig);
            _commandRepositoryMock.SetupEntities(commands);

            var result = Sut.GetAllowedCommands(null);

            CollectionAssert.IsEmpty(result);
        }

        [Test]
        public void GetAllowedCommands_ShouldReturnAllCommands_WhenRoleIsEmptyAndNoAuthenticationMethodDefined()
        {
            var commands = Builder<DataCommand>.CreateListOfSize(5).Build();

            _accountRepositoryMock.SetupEntities(Enumerable.Empty<Account>());
            _pluginConfigurationRepositoryMock.SetupEntities(Enumerable.Empty<PluginConfiguration>());
            _commandRepositoryMock.SetupEntities(commands);

            var result = Sut.GetAllowedCommands(null);

            CollectionAssert.AreEquivalent(commands.Select(x => x.Name), result);
        }

        [Test]
        public void GetAllowedCommands_ShouldReturnAllCommands_WhenRoleIsAdmin()
        {
            var role = new DataRole
            {
                ExternalId = Guid.NewGuid(),
                Name = "Admin"
            };
            var commands = Builder<DataCommand>.CreateListOfSize(5).Build();

            _roleRepositoryMock.SetupEntities(new[] { role });
            _commandRepositoryMock.SetupEntities(commands);

            var result = Sut.GetAllowedCommands(role.ExternalId);

            CollectionAssert.AreEquivalent(commands.Select(x => x.Name), result);
        }

        private static IList<DataCommand> BuildCommandEntities()
        {
            var i = 1000;
            return Builder<DataCommand>.CreateListOfSize(5)
                .All()
                .Do(x => x.CommandId = i++)
                .Do(x => x.Description = RandomData.RandomString(10))
                .Do(x => x.Group = RandomData.RandomString(10))
                .Do(x => x.Name = RandomData.RandomString(10))
                .Do(x => x.IsBackground = RandomData.RandomBool())
                .Do(x => x.CommandPermissions = RandomData.RandomBool()
                    ? new List<DataCommandPermission>()
                    : Builder<DataCommandPermission>.CreateListOfSize(RandomData.RandomInt(1, 5))
                    .All()
                    .Do(cp => cp.Role = new DataRole
                    {
                        ExternalId = Guid.NewGuid(),
                        Description = RandomData.RandomString(10),
                        Name = RandomData.RandomString(10),
                        Id = RandomData.RandomInt(10000)
                    })
                    .Build())
                .Build();
        }

        private static IList<DataRole> BuildRoleEntities()
        {
            var i = 1000;
            return Builder<DataRole>.CreateListOfSize(5)
                .All()
                .Do(x => x.Id = i++)
                .Do(x => x.Description = RandomData.RandomString(10))
                .Do(x => x.Name = RandomData.RandomString(10))
                .Do(x => x.ExternalId = Guid.NewGuid())
                .Build();
        }
    }
}
