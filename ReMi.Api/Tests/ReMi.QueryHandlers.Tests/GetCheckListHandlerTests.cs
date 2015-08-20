using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Queries.ReleasePlan;
using ReMi.QueryHandlers.ReleasePlan;

namespace ReMi.QueryHandlers.Tests
{
    public class GetCheckListHandlerTests : TestClassFor<GetCheckListHandler>
    {
        private Mock<ICheckListGateway> _checkListGatewayMock;

        protected override GetCheckListHandler ConstructSystemUnderTest()
        {
            return new GetCheckListHandler
            {
                CheckListGatewayFactory = () => _checkListGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _checkListGatewayMock = new Mock<ICheckListGateway>();

            base.TestInitialize();
        }

        [Test]
        public void GetAdditionalQuestions_ShouldReturnThemCorrectly()
        {
            var question = new CheckListQuestion
            {
                Question = RandomData.RandomString(13)
            };
            var releaseWindowId = Guid.NewGuid();
            _checkListGatewayMock.Setup(c => c.GetCheckListAdditionalQuestions(releaseWindowId))
                .Returns(new List<CheckListQuestion> { question });

            var result = Sut.Handle(new CheckListAdditionalQuestionRequest {ReleaseWindowId = releaseWindowId});

            _checkListGatewayMock.Verify(c => c.GetCheckListAdditionalQuestions(releaseWindowId));
            Assert.AreEqual(question, result.Questions[0]);
        }

        [Test]
        public void GetChecklist_ShouldReturnCorrectResult()
        {
            var checkList = new List<CheckListItemView> { new CheckListItemView { ExternalId = Guid.NewGuid() } };
            var releaseWindowId = Guid.NewGuid();
            _checkListGatewayMock.Setup(c => c.GetCheckList(releaseWindowId)).Returns(checkList);

            var result = Sut.Handle(new GetCheckListRequest {ReleaseWindowId = releaseWindowId});

            _checkListGatewayMock.Verify(c => c.GetCheckList(releaseWindowId));
            Assert.AreEqual(checkList[0].ExternalId, result.CheckList.ToList()[0].ExternalId);
        }
    }
}
