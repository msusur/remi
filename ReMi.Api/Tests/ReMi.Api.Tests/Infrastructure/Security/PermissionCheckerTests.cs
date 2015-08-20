using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.Api.Insfrastructure.Security;
using ReMi.BusinessEntities.Auth;
using ReMi.Common.Constants.Auth;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.TestUtils.UnitTests;
using System;
using System.Collections.Generic;

namespace ReMi.Api.Tests.Infrastructure.Security
{
    [TestFixture]
    public class PermissionCheckerTests : TestClassFor<PermissionChecker>
    {
        private Mock<ISecurityGateway> _securityGatewayMock;
        private Mock<IAuthorizationManager> _authorizationManagerMock;
        private Mock<ICommand> _commandMock;
        private Mock<IQuery> _queryMock;

        protected override PermissionChecker ConstructSystemUnderTest()
        {
            _securityGatewayMock = new Mock<ISecurityGateway>(MockBehavior.Strict);
            _securityGatewayMock.Setup(x => x.Dispose());
            _authorizationManagerMock = new Mock<IAuthorizationManager>(MockBehavior.Strict);
            _commandMock = new Mock<ICommand>(MockBehavior.Strict);
            _queryMock = new Mock<IQuery>(MockBehavior.Strict);

            return new PermissionChecker
            {
                SecurityGateway = () => _securityGatewayMock.Object,
                AuthorizationManager = _authorizationManagerMock.Object
            };
        }

        [Test]
        public void CheckCommandPermission_ShouldReturnStatusPermitted_WhenAccountRoleIsAdmin()
        {
            var account = BuildAccount();
            account.Role.Name = "Admin";
            var commandType = _commandMock.Object.GetType();

            var result = Sut.CheckCommandPermission(commandType, account);

            Assert.AreEqual(PermissionStatus.Permmited, result);
            _securityGatewayMock.Verify(x => x.GetCommandRoles(commandType.Name), Times.Never);
            _authorizationManagerMock.Verify(x => x.IsAuthorized(It.IsAny<IEnumerable<Role>>()), Times.Never);
        }

        [Test]
        public void CheckCommandPermission_ShouldReturnStatusPermitted_WhenAccountRoleHasPermission()
        {
            var account = BuildAccount();
            var commandType = _commandMock.Object.GetType();

            _securityGatewayMock.Setup(x => x.GetCommandRoles(commandType.Name))
                .Returns(new[] { new Role { Name = "Admin" } });
            _authorizationManagerMock.Setup(x => x.IsAuthorized(It.IsAny<IEnumerable<Role>>()))
                .Returns(true);

            var result = Sut.CheckCommandPermission(commandType, account);

            Assert.AreEqual(PermissionStatus.Permmited, result);
            _securityGatewayMock.Verify(x => x.GetCommandRoles(commandType.Name), Times.Once());
            _authorizationManagerMock.Verify(x => x.IsAuthorized(It.IsAny<IEnumerable<Role>>()), Times.Once());
        }

        [Test]
        public void CheckCommandPermission_ShouldReturnStatusNotAuthenticated_WhenAccountIsNull()
        {
            var commandType = _commandMock.Object.GetType();

            _securityGatewayMock.Setup(x => x.GetCommandRoles(commandType.Name))
                .Returns(new[] { new Role { Name = "Admin" } });

            var result = Sut.CheckCommandPermission(commandType, null);

            Assert.AreEqual(PermissionStatus.NotAuthenticated, result);
            _securityGatewayMock.Verify(x => x.GetCommandRoles(commandType.Name), Times.Once());
        }

        [Test]
        public void CheckCommandPermission_ShouldReturnStatusPermitted_WhenAccountIsNullAndOneOfRolesIsNotAuthenticated()
        {
            var commandType = _commandMock.Object.GetType();

            _securityGatewayMock.Setup(x => x.GetCommandRoles(commandType.Name))
                .Returns(new[] { new Role { Name = "Admin" }, new Role { Name = "NotAuthenticated" } });

            var result = Sut.CheckCommandPermission(commandType, null);

            Assert.AreEqual(PermissionStatus.Permmited, result);
            _securityGatewayMock.Verify(x => x.GetCommandRoles(commandType.Name), Times.Once());
        }

        [Test]
        public void CheckCommandPermission_ShouldReturnStatusNotAuthorized_WhenAccountRoleHasNoPermission()
        {
            var account = BuildAccount();
            var commandType = _commandMock.Object.GetType();

            _securityGatewayMock.Setup(x => x.GetCommandRoles(commandType.Name))
                .Returns(new[] { new Role { Name = "ProductionSupport" }, new Role { Name = "NoNotAuthenticatedt" } });
            _authorizationManagerMock.Setup(x => x.IsAuthorized(It.IsAny<IEnumerable<Role>>()))
                .Returns(false);

            var result = Sut.CheckCommandPermission(commandType, account);

            Assert.AreEqual(PermissionStatus.NotAuthorized, result);
            _securityGatewayMock.Verify(x => x.GetCommandRoles(commandType.Name), Times.Once());
            _authorizationManagerMock.Verify(x => x.IsAuthorized(It.IsAny<IEnumerable<Role>>()), Times.Once());
        }

        [Test]
        public void CheckCommandPermission_ShouldReturnStatusNotAuthorized_WhenCommandHasNoPermissions()
        {
            var account = BuildAccount();
            var commandType = _commandMock.Object.GetType();

            _securityGatewayMock.Setup(x => x.GetCommandRoles(commandType.Name))
                .Returns((IEnumerable<Role>)null);

            var result = Sut.CheckCommandPermission(commandType, account);

            Assert.AreEqual(PermissionStatus.NotAuthorized, result);
            _securityGatewayMock.Verify(x => x.GetCommandRoles(commandType.Name), Times.Once());
        }

        [Test]
        public void CheckQueryPermission_ShouldReturnStatusPermitted_WhenAccountRoleIsAdmin()
        {
            var account = BuildAccount();
            account.Role.Name = "Admin";
            var queryType = _queryMock.Object.GetType();

            var result = Sut.CheckQueryPermission(queryType, account);

            Assert.AreEqual(PermissionStatus.Permmited, result);
            _securityGatewayMock.Verify(x => x.GetQueryRoles(queryType.Name), Times.Never);
            _authorizationManagerMock.Verify(x => x.IsAuthorized(It.IsAny<IEnumerable<Role>>()), Times.Never);
        }

        [Test]
        public void CheckQueryPermission_ShouldReturnStatusPermitted_WhenAccountRoleHasPermission()
        {
            var account = BuildAccount();
            var queryType = _queryMock.Object.GetType();

            _securityGatewayMock.Setup(x => x.GetQueryRoles(queryType.Name))
                .Returns(new[] { new Role { Name = "Admin" } });
            _authorizationManagerMock.Setup(x => x.IsAuthorized(It.IsAny<IEnumerable<Role>>()))
                .Returns(true);

            var result = Sut.CheckQueryPermission(queryType, account);

            Assert.AreEqual(PermissionStatus.Permmited, result);
            _securityGatewayMock.Verify(x => x.GetQueryRoles(queryType.Name), Times.Once());
            _authorizationManagerMock.Verify(x => x.IsAuthorized(It.IsAny<IEnumerable<Role>>()), Times.Once());
        }

        [Test]
        public void CheckQueryPermission_ShouldReturnStatusPermitted_WhenQueryHasNoRoleAssigned()
        {
            var account = BuildAccount();
            var queryType = _queryMock.Object.GetType();

            _securityGatewayMock.Setup(x => x.GetQueryRoles(queryType.Name))
                .Returns((IEnumerable<Role>)null);

            var result = Sut.CheckQueryPermission(queryType, account);

            Assert.AreEqual(PermissionStatus.Permmited, result);
            _securityGatewayMock.Verify(x => x.GetQueryRoles(queryType.Name), Times.Once());
        }

        [Test]
        public void CheckQueryPermission_ShouldReturnStatusNotAuthenticated_WhenAccountIsNull()
        {
            var queryType = _queryMock.Object.GetType();

            _securityGatewayMock.Setup(x => x.GetQueryRoles(queryType.Name))
                .Returns(new[] { new Role { Name = "Admin" } });

            var result = Sut.CheckQueryPermission(queryType, null);

            Assert.AreEqual(PermissionStatus.NotAuthenticated, result);
            _securityGatewayMock.Verify(x => x.GetQueryRoles(queryType.Name), Times.Once());
        }

        [Test]
        public void CheckQueryPermission_ShouldReturnStatusPermitted_WhenAccountIsNullAndOneOfRolesIsNotAuthenticated()
        {
            var queryType = _queryMock.Object.GetType();

            _securityGatewayMock.Setup(x => x.GetQueryRoles(queryType.Name))
                .Returns(new[] { new Role { Name = "Admin" }, new Role { Name = "NotAuthenticated" } });

            var result = Sut.CheckQueryPermission(queryType, null);

            Assert.AreEqual(PermissionStatus.Permmited, result);
            _securityGatewayMock.Verify(x => x.GetQueryRoles(queryType.Name), Times.Once());
        }


        [Test]
        public void CheckQueryPermission_ShouldReturnStatusNotAuthorized_WhenAccountRoleHasNoPermission()
        {
            var account = BuildAccount();
            var queryType = _queryMock.Object.GetType();

            _securityGatewayMock.Setup(x => x.GetQueryRoles(queryType.Name))
                .Returns(new[] { new Role { Name = "ProductionSupport" }, new Role { Name = "NoNotAuthenticatedt" } });
            _authorizationManagerMock.Setup(x => x.IsAuthorized(It.IsAny<IEnumerable<Role>>()))
                .Returns(false);

            var result = Sut.CheckQueryPermission(queryType, account);

            Assert.AreEqual(PermissionStatus.NotAuthorized, result);
            _securityGatewayMock.Verify(x => x.GetQueryRoles(queryType.Name), Times.Once());
            _authorizationManagerMock.Verify(x => x.IsAuthorized(It.IsAny<IEnumerable<Role>>()), Times.Once());
        }

        private static Account BuildAccount()
        {
            return Builder<Account>.CreateNew()
                .With(x => x.FullName, RandomData.RandomString(5))
                .With(x => x.ExternalId, Guid.NewGuid())
                .With(x => x.Role, new Role { Name = "TeamMember" })
                .Build();
        }
    }
}
