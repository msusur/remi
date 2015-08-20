using System;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ProductRequests;
using ReMi.CommandHandlers.ProductRequests;
using ReMi.Commands.ProductRequests;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataAccess.BusinessEntityGateways.ProductRequests;

namespace ReMi.CommandHandlers.Tests.ProductRequests
{
    public class UpdateProductRequestGroupCommandHandlerTests : TestClassFor<UpdateProductRequestGroupCommandHandler>
    {
        private Mock<IProductRequestGateway> _productRequestGatewayMock;
        private Mock<IProductRequestAssigneeGateway> _productRequestAssigneeGatewayMock;
        private Mock<IAccountsGateway> _accountsGatewayMock;

        protected override UpdateProductRequestGroupCommandHandler ConstructSystemUnderTest()
        {
            return new UpdateProductRequestGroupCommandHandler
            {
                ProductRequestGatewayFactory = () => _productRequestGatewayMock.Object,
                ProductRequestAssigneeGatewayFactory = () => _productRequestAssigneeGatewayMock.Object,
                AccountsGatewayFactory = () => _accountsGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _productRequestGatewayMock = new Mock<IProductRequestGateway>();
            _productRequestAssigneeGatewayMock = new Mock<IProductRequestAssigneeGateway>();
            _accountsGatewayMock = new Mock<IAccountsGateway>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallGateway_WhenInvoked()
        {
            var group = Builder<ProductRequestGroup>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.Assignees, new Account[0])
                .Build();

            var command = new UpdateProductRequestGroupCommand
            {
                RequestGroup = group
            };

            Sut.Handle(command);

            _productRequestGatewayMock.Verify(o => o.UpdateProductRequestGroup(group));
        }

        [Test]
        public void Handle_ShouldCallGateway_WhenAssigneesIsNull()
        {
            var group = Builder<ProductRequestGroup>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.Assignees, null)
                .Build();

            var command = new UpdateProductRequestGroupCommand
            {
                RequestGroup = group
            };

            Sut.Handle(command);

            _productRequestGatewayMock.Verify(o => o.UpdateProductRequestGroup(group));
        }

        [Test]
        public void Handle_ShouldInsertAssignees_WhenNewAssigneesAttached()
        {
            var group = Builder<ProductRequestGroup>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.Assignees, new[]
                {
                    new Account{ ExternalId = Guid.NewGuid(), FullName = RandomData.RandomString(1, 20)}, 
                    new Account{ ExternalId = Guid.NewGuid(), FullName = RandomData.RandomString(1, 20)}
                })
                .Build();

            _accountsGatewayMock.Setup(o => o.GetAccount(group.Assignees.ElementAt(0).ExternalId, false))
                .Returns(group.Assignees.ElementAt(0));
            _accountsGatewayMock.Setup(o => o.GetAccount(group.Assignees.ElementAt(1).ExternalId, false))
                .Returns(group.Assignees.ElementAt(1));

            var command = new UpdateProductRequestGroupCommand
            {
                RequestGroup = group,
            };

            Sut.Handle(command);

            _productRequestAssigneeGatewayMock.Verify(o => o.GetAssignees(group.ExternalId));
            _productRequestAssigneeGatewayMock.Verify(o => o.AppendAssignee(group.ExternalId, group.Assignees.ElementAt(0).ExternalId));
            _productRequestAssigneeGatewayMock.Verify(o => o.AppendAssignee(group.ExternalId, group.Assignees.ElementAt(1).ExternalId));

            _productRequestAssigneeGatewayMock.Verify(o => o.AppendAssignee(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(2));
            _productRequestAssigneeGatewayMock.Verify(o => o.RemoveAssignee(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(0));
        }

        [Test]
        public void Handle_ShouldCreateNewAccount_WhenNewAccountSelectedFromService()
        {
            var group = Builder<ProductRequestGroup>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.Assignees, new[]
                {
                    new Account{ExternalId = Guid.NewGuid(), FullName = RandomData.RandomString(1, 100), Email = RandomData.RandomEmail()},
                    new Account{ExternalId = Guid.NewGuid(), FullName = RandomData.RandomString(1, 100), Email = RandomData.RandomEmail()}
                })
                .Build();

            _accountsGatewayMock.Setup(o => o.CreateAccount(group.Assignees.ElementAt(0), false))
                .Returns(group.Assignees.ElementAt(0));
            _accountsGatewayMock.Setup(o => o.CreateAccount(group.Assignees.ElementAt(1), false))
                .Returns(group.Assignees.ElementAt(1));

            var command = new UpdateProductRequestGroupCommand
            {
                RequestGroup = group
            };

            Sut.Handle(command);

            _accountsGatewayMock.Verify(o => o.GetAccount(It.IsAny<Guid>(), false), Times.Exactly(2));
            _accountsGatewayMock.Verify(o => o.CreateAccount(group.Assignees.ElementAt(0), false), Times.Exactly(1));
            _accountsGatewayMock.Verify(o => o.CreateAccount(group.Assignees.ElementAt(1), false), Times.Exactly(1));
        }

        [Test]
        public void Handle_ShouldRemoveAssignees_WhenAssigneesNotExistsInCommand()
        {
            var group = Builder<ProductRequestGroup>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.Assignees, null)
                .Build();

            var command = new UpdateProductRequestGroupCommand
            {
                RequestGroup = group
            };

            var existinAssigness = new[]
            {
                new Account{ ExternalId = Guid.NewGuid(), FullName = RandomData.RandomString(1, 20)}, 
                new Account{ ExternalId = Guid.NewGuid(), FullName = RandomData.RandomString(1, 20)}
            };
            _productRequestAssigneeGatewayMock.Setup(o => o.GetAssignees(group.ExternalId))
                .Returns(existinAssigness);

            Sut.Handle(command);

            _productRequestAssigneeGatewayMock.Verify(o => o.GetAssignees(group.ExternalId));
            _productRequestAssigneeGatewayMock.Verify(o => o.RemoveAssignee(group.ExternalId, existinAssigness.ElementAt(0).ExternalId));
            _productRequestAssigneeGatewayMock.Verify(o => o.RemoveAssignee(group.ExternalId, existinAssigness.ElementAt(1).ExternalId));

            _productRequestAssigneeGatewayMock.Verify(o => o.AppendAssignee(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            _productRequestAssigneeGatewayMock.Verify(o => o.RemoveAssignee(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(2));
        }
    }
}
