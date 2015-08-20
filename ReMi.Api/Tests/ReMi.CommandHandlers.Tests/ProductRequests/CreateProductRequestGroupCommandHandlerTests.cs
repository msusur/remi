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
    public class CreateProductRequestGroupCommandHandlerTests : TestClassFor<CreateProductRequestGroupCommandHandler>
    {
        private Mock<IProductRequestGateway> _productRequestGatewayMock;
        private Mock<IProductRequestAssigneeGateway> _productRequestAssigneeGatewayMock;
        private Mock<IAccountsGateway> _accountsGatewayMock;

        protected override CreateProductRequestGroupCommandHandler ConstructSystemUnderTest()
        {
            return new CreateProductRequestGroupCommandHandler
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

            var command = new CreateProductRequestGroupCommand
            {
                RequestGroup = group
            };

            Sut.Handle(command);

            _productRequestGatewayMock.Verify(o => o.CreateProductRequestGroup(group));
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

            var command = new CreateProductRequestGroupCommand
            {
                RequestGroup = group
            };

            Sut.Handle(command);

            _accountsGatewayMock.Verify(o => o.GetAccount(It.IsAny<Guid>(), false), Times.Exactly(2));
            _accountsGatewayMock.Verify(o => o.CreateAccount(group.Assignees.ElementAt(0), false), Times.Exactly(1));
            _accountsGatewayMock.Verify(o => o.CreateAccount(group.Assignees.ElementAt(1), false), Times.Exactly(1));
        }

        [Test]
        public void Handle_ShouldAppendAssignees_WhenInvoked()
        {
            var group = Builder<ProductRequestGroup>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.Assignees, new[]
                {
                    new Account{ExternalId = Guid.NewGuid(), FullName = RandomData.RandomString(1, 100)},
                    new Account{ExternalId = Guid.NewGuid(), FullName = RandomData.RandomString(1, 100)},
                })
                .Build();

            _accountsGatewayMock.Setup(o => o.GetAccount(group.Assignees.ElementAt(0).ExternalId, false))
                .Returns(group.Assignees.ElementAt(0));
            _accountsGatewayMock.Setup(o => o.GetAccount(group.Assignees.ElementAt(1).ExternalId, false))
                .Returns(group.Assignees.ElementAt(1));

            var command = new CreateProductRequestGroupCommand
            {
                RequestGroup = group
            };

            Sut.Handle(command);

            _productRequestAssigneeGatewayMock.Verify(o => o.AppendAssignee(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(2));
            _productRequestAssigneeGatewayMock.Verify(o => o.AppendAssignee(group.ExternalId, group.Assignees.ElementAt(0).ExternalId), Times.Exactly(1));
            _productRequestAssigneeGatewayMock.Verify(o => o.AppendAssignee(group.ExternalId, group.Assignees.ElementAt(1).ExternalId), Times.Exactly(1));
        }
    }
}
