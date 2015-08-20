using System;
using Moq;
using NUnit.Framework;
using ReMi.CommandHandlers.ReleasePlan;
using ReMi.Commands.ReleasePlan;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;

namespace ReMi.CommandHandlers.Tests.ReleasePlan
{
    public class DeleteReleaseTaskAttachmentCommandHandlerTests
        : TestClassFor<DeleteReleaseTaskAttachmentCommandHandler>
    {
        private Mock<IReleaseTaskGateway> _releaseTaskGatewayMock;

        protected override DeleteReleaseTaskAttachmentCommandHandler ConstructSystemUnderTest()
        {
            return new DeleteReleaseTaskAttachmentCommandHandler
            {
                ReleaseTaskGatewayFactory = () => _releaseTaskGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseTaskGatewayMock = new Mock<IReleaseTaskGateway>();

            base.TestInitialize();
        }

        [Test]
        public void DeleteReleaseTaskAttachment_ShouldCallGatewayMethodAndDisposeIt_WhenCallCallGateway()
        {
            var attachmentId = Guid.NewGuid();
            
            Sut.Handle(new DeleteReleaseTaskAttachmentCommand{ReleaseTaskAttachmentId = attachmentId});

            _releaseTaskGatewayMock.Verify(x => x.DeleteReleaseTaskAttachment(attachmentId));
            _releaseTaskGatewayMock.Verify(x => x.Dispose());
        }
    }
}
