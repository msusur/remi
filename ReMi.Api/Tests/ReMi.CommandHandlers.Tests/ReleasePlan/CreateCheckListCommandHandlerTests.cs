using System;
using Moq;
using NUnit.Framework;
using ReMi.CommandHandlers.ReleasePlan;
using ReMi.Commands.ReleasePlan;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;

namespace ReMi.CommandHandlers.Tests.ReleasePlan
{
    public class CreateCheckListCommandHandlerTests : TestClassFor<CreateCheckListCommandHandler>
    {
        private Mock<ICheckListGateway> _checkListGatewayMock;

        protected override CreateCheckListCommandHandler ConstructSystemUnderTest()
        {
            return new CreateCheckListCommandHandler {CheckListGatewayFactory = () => _checkListGatewayMock.Object};
        }

        protected override void TestInitialize()
        {
            _checkListGatewayMock = new Mock<ICheckListGateway>();

            base.TestInitialize();
        }

        [Test]
        public void CreateChecklist_ShouldCallGateway()
        {
            var releaseWindowId = Guid.NewGuid();

            Sut.Handle(new CreateCheckListCommand {ReleaseWindowId = releaseWindowId});

            _checkListGatewayMock.Verify(c => c.Create(releaseWindowId));
        }
    }
}
