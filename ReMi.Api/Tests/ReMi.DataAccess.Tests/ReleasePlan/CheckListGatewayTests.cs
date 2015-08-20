using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Common.Utils.Repository;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.Products;
using ReMi.DataEntities.ReleaseCalendar;
using ReMi.DataEntities.ReleasePlan;
using BusinessCheckListQuestion = ReMi.BusinessEntities.ReleasePlan.CheckListQuestion;
using CheckListQuestion = ReMi.DataEntities.ReleasePlan.CheckListQuestion;

namespace ReMi.DataAccess.Tests.ReleasePlan
{
    public class CheckListGatewayTests : TestClassFor<CheckListGateway>
    {
        private Mock<IRepository<CheckListQuestion>> _checkListQuestionRepositoryMock;
        private Mock<IRepository<CheckListQuestionToProduct>> _checkListQuestionToProductRepositoryMock;
        private Mock<IRepository<CheckList>> _checkListRepositoryMock;
        private Mock<IRepository<ReleaseWindow>> _releaseWindowRepositoryMock;

        protected override CheckListGateway ConstructSystemUnderTest()
        {
            return new CheckListGateway
            {
                CheckListQuestionRepository = _checkListQuestionRepositoryMock.Object,
                CheckListQuestionToProductRepository = _checkListQuestionToProductRepositoryMock.Object,
                CheckListRepository = _checkListRepositoryMock.Object,
                ReleaseWindowRepository = _releaseWindowRepositoryMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _checkListQuestionRepositoryMock = new Mock<IRepository<CheckListQuestion>>(MockBehavior.Strict);
            _checkListQuestionToProductRepositoryMock = new Mock<IRepository<CheckListQuestionToProduct>>(MockBehavior.Strict);
            _checkListRepositoryMock = new Mock<IRepository<CheckList>>(MockBehavior.Strict);
            _releaseWindowRepositoryMock = new Mock<IRepository<ReleaseWindow>>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Dispose_ShouldDisposeAllRepositories_WhenInvoked()
        {
            _checkListQuestionRepositoryMock.Setup(x => x.Dispose());
            _checkListQuestionToProductRepositoryMock.Setup(x => x.Dispose());
            _checkListRepositoryMock.Setup(x => x.Dispose());
            _releaseWindowRepositoryMock.Setup(x => x.Dispose());

            Sut.Dispose();

            _checkListQuestionRepositoryMock.Verify(x => x.Dispose(), Times.Once);
            _checkListQuestionToProductRepositoryMock.Verify(x => x.Dispose(), Times.Once);
            _checkListRepositoryMock.Verify(x => x.Dispose(), Times.Once);
            _releaseWindowRepositoryMock.Verify(x => x.Dispose(), Times.Once);
        }

        [Test]
        public void GetCheckList_ShouldReturnNull_WhenReleaseWindowWasNotFound()
        {
            _releaseWindowRepositoryMock.SetupEntities(new List<ReleaseWindow>
            {
                new ReleaseWindow {ExternalId = Guid.NewGuid()}
            });

            var result = Sut.GetCheckList(Guid.NewGuid());

            Assert.IsNull(result);
        }

        [Test]
        public void GetCheckList_ShouldReturnCheckList_WhenReleaseWindowWasFound()
        {
            var releaseWindowId = Guid.NewGuid();
            var releaseWindow = new ReleaseWindow
            {
                ExternalId = releaseWindowId,
                CheckList = new Collection<CheckList>
                {
                    new CheckList
                    {
                        CheckListQuestion = new CheckListQuestion {Content = RandomData.RandomString(12)},
                        Comment = RandomData.RandomString(11),
                        ExternalId = Guid.NewGuid(),
                        ReleaseWindow = new ReleaseWindow {ExternalId = releaseWindowId}
                    }
                }
            };
            _releaseWindowRepositoryMock.SetupEntities(new List<ReleaseWindow> { releaseWindow });

            var result = Sut.GetCheckList(releaseWindowId).ToList();

            Assert.IsNotNull(result, "Result is null");
            Assert.AreEqual(1, result.Count, "Should return only one checklist");
            Assert.IsFalse(result[0].Checked, "Checked");
            Assert.AreEqual(releaseWindow.CheckList.ToList()[0].CheckListQuestion.Content, result[0].CheckListQuestion,
                "CheckListQuestion");
            Assert.AreEqual(releaseWindow.CheckList.ToList()[0].Comment, result[0].Comment, "Comment");
            Assert.AreEqual(releaseWindowId, result[0].ReleaseWindowId, "ReleaseWindowId");
            Assert.AreEqual(releaseWindow.CheckList.ToList()[0].ExternalId, result[0].ExternalId, "ExternalId");
        }

        [Test]
        public void GetAdditionalQuestions_ShouldReturnCorrectResult()
        {
            var releaseWindowId = Guid.NewGuid();
            var otherQuestion = new CheckListQuestion
            {
                Content = RandomData.RandomString(11),
                ExternalId = Guid.NewGuid(),
                CheckLists =
                    new List<CheckList>
                    {
                        new CheckList {ReleaseWindow = new ReleaseWindow {ExternalId = Guid.NewGuid()}}
                    }
            };
            _checkListQuestionRepositoryMock.SetupEntities(new List<CheckListQuestion> { otherQuestion });

            var result = Sut.GetCheckListAdditionalQuestions(releaseWindowId);

            Assert.IsNotNull(result, "Null");
            Assert.AreEqual(1, result.Count, "result list size");
            Assert.AreEqual(otherQuestion.Content, result[0].Question, "content");
            Assert.AreEqual(otherQuestion.ExternalId, result[0].ExternalId, "ExternalId");
        }

        [Test]
        public void GetAllQuestions_ShouldReturnCorrectResult()
        {
            var question = new CheckListQuestion { Content = RandomData.RandomString(11) };
            var otherQuestion = new CheckListQuestion { Content = RandomData.RandomString(11) };
            _checkListQuestionRepositoryMock.SetupEntities(new List<CheckListQuestion> { question, otherQuestion });

            var result = Sut.GetCheckListQuestions();

            Assert.IsNotNull(result, "Null");
            Assert.AreEqual(2, result.Count, "result list size");
            Assert.AreEqual(question.Content, result[0], "content");
            Assert.AreEqual(otherQuestion.Content, result[1], "content");
        }

        [Test]
        public void Create_ShouldInsertNewCheckListToRepository()
        {
            var productId = RandomData.RandomInt(int.MaxValue);
            var product = RandomData.RandomString(5);
            var releaseWindowId = Guid.NewGuid();
            var releaseWindow = new ReleaseWindow
            {
                ExternalId = releaseWindowId,
                ReleaseWindowId = RandomData.RandomInt(231),
                ReleaseProducts = new[] { new ReleaseProduct
                {
                    Product = Builder<Product>.CreateNew()
                        .With(x => x.Description = product)
                        .With(x => x.ExternalId = Guid.NewGuid())
                        .With(x => x.ProductId= productId)
                        .Build()
                } }
            };
            _releaseWindowRepositoryMock.SetupEntities(new List<ReleaseWindow> { releaseWindow });
            var question = new CheckListQuestion
            {
                CheckListQuestionId = RandomData.RandomInt(453),
                Content = RandomData.RandomString(23)
            };
            var otherQuestion = new CheckListQuestion
            {
                CheckListQuestionId = RandomData.RandomInt(453),
                Content = RandomData.RandomString(23)
            };
            _checkListQuestionRepositoryMock.SetupEntities(new List<CheckListQuestion> { question, otherQuestion });
            _checkListQuestionToProductRepositoryMock.SetupEntities(new List<CheckListQuestionToProduct>
            {
                new CheckListQuestionToProduct
                {
                    CheckListQuestionId = question.CheckListQuestionId,
                    ProductId = productId,
                    Product = Builder<Product>.CreateNew().With(x => x.Description = product)
                        .With(x => x.ExternalId = Guid.NewGuid())
                        .Build()
                },
                new CheckListQuestionToProduct
                {
                    CheckListQuestionId = otherQuestion.CheckListQuestionId,
                    Product = Builder<Product>.CreateNew().With(x => x.Description = RandomData.RandomString(5))
                        .With(x => x.ExternalId = Guid.NewGuid())
                        .Build()
                }
            });
            _checkListRepositoryMock.Setup(
                ch => ch.Insert(It.Is<CheckList>(c =>
                    c.ReleaseWindowId == releaseWindow.ReleaseWindowId &&
                    c.CheckListQuestionId == question.CheckListQuestionId)));

            Sut.Create(releaseWindowId);

            _checkListRepositoryMock.Verify(ch => ch.Insert(It.IsAny<CheckList>()), Times.Once);
        }

        [Test]
        public void UpdateAnswer_ShouldUpdateTheAnswerCorrectly()
        {
            var checkList = new CheckList { ExternalId = Guid.NewGuid() };
            var updatedCheckList = new CheckListItem
            {
                ExternalId = checkList.ExternalId,
                Checked = true,
                LastChangedBy = RandomData.RandomString(32)
            };
            _checkListRepositoryMock.SetupEntities(new List<CheckList> { checkList });
            _checkListRepositoryMock.Setup(c => c.Update(It.Is<CheckList>(ch =>
                ch.Checked == updatedCheckList.Checked
                && ch.ExternalId == checkList.ExternalId
                && ch.LastChangedBy == updatedCheckList.LastChangedBy)))
                .Returns((ChangedFields<CheckList>)null);

            Sut.UpdateAnswer(updatedCheckList);

            _checkListRepositoryMock.Verify(c => c.Update(It.IsAny<CheckList>()), Times.Once);
        }

        [Test]
        public void UpdateComment_ShouldUpdateTheAnswerCorrectly()
        {
            var checkList = new CheckList { ExternalId = Guid.NewGuid() };
            var updatedCheckList = new CheckListItem
            {
                ExternalId = checkList.ExternalId,
                Comment = RandomData.RandomString(34),
                LastChangedBy = RandomData.RandomString(32)
            };
            _checkListRepositoryMock.SetupEntities(new List<CheckList> { checkList });
            _checkListRepositoryMock.Setup(c => c.Update(It.Is<CheckList>(ch =>
                ch.Comment == updatedCheckList.Comment
                && ch.ExternalId == checkList.ExternalId
                && ch.LastChangedBy == updatedCheckList.LastChangedBy)))
                .Returns((ChangedFields<CheckList>)null);

            Sut.UpdateComment(updatedCheckList);

            _checkListRepositoryMock.Verify(c => c.Update(It.IsAny<CheckList>()), Times.Once);
        }

        [Test]
        public void GetCheckListItem_ShouldReturnCorrectValue()
        {
            var checkList = new CheckList
            {
                ExternalId = Guid.NewGuid(),
                Comment = RandomData.RandomString(89),
                CheckListQuestion = new CheckListQuestion { Content = RandomData.RandomString(56, 99) },
                Checked = RandomData.RandomBool(),
                LastChangedBy = RandomData.RandomString(22, 44),
                ReleaseWindow = new ReleaseWindow { ExternalId = Guid.NewGuid() }
            };
            _checkListRepositoryMock.SetupEntities(new List<CheckList>
            {
                checkList,
                new CheckList
                {
                    ReleaseWindow = new ReleaseWindow {ExternalId = Guid.NewGuid()},
                    ExternalId = Guid.NewGuid()
                }
            });

            var result = Sut.GetCheckListItem(checkList.ExternalId);

            Assert.AreEqual(checkList.ReleaseWindow.ExternalId, result.ReleaseWindowId, "Release window external id");
            Assert.AreEqual(checkList.ExternalId, result.ExternalId, "check list external id");
            Assert.AreEqual(checkList.Comment, result.Comment, "comment");
            Assert.AreEqual(checkList.Checked, result.Checked, "checked");
            Assert.AreEqual(checkList.LastChangedBy, result.LastChangedBy, "last changed by");
            Assert.AreEqual(checkList.CheckListQuestion.Content, result.CheckListQuestion, "question");
        }

        [Test]
        public void AddCheckListQuestions_ShouldDoNothing_WhenQuestionListIsEmptyOrNull()
        {
            Sut.AddCheckListQuestions(null, Guid.NewGuid());
            Sut.AddCheckListQuestions(Enumerable.Empty<BusinessCheckListQuestion>(), Guid.NewGuid());

            _checkListQuestionRepositoryMock.Verify(x => x.Insert(It.IsAny<IEnumerable<CheckListQuestion>>()), Times.Never);
        }

        [Test]
        public void AddCheckListQuestions_ShouldInsertNewQuestions_WhenInvoked()
        {
            var questions = Builder<BusinessCheckListQuestion>.CreateListOfSize(5).Build();
            var releaseWindow = Builder<ReleaseWindow>.CreateNew()
                .With(x => x.ReleaseProducts,Enumerable.Empty<ReleaseProduct>())
                .Build();

            _checkListQuestionRepositoryMock.Setup(x => x.Insert(It.Is<IEnumerable<CheckListQuestion>>(c => 
                c.Count() == 5
                && c.All(dq => questions.Any(bq => bq.ExternalId == dq.ExternalId))
                && c.All(dq => questions.Any(bq => bq.Question == dq.Content)))));

            _releaseWindowRepositoryMock.SetupEntities(new[] { releaseWindow });
            _checkListQuestionRepositoryMock.SetupEntities(Enumerable.Empty<CheckListQuestion>());
            _checkListQuestionToProductRepositoryMock.Setup(x => x.Insert(It.IsAny<IEnumerable<CheckListQuestionToProduct>>()));
            _checkListRepositoryMock.Setup(x => x.Insert(It.IsAny<IEnumerable<CheckList>>()));

            Sut.AddCheckListQuestions(questions, releaseWindow.ExternalId);

            _checkListQuestionRepositoryMock.Verify(x => x.Insert(It.IsAny<IEnumerable<CheckListQuestion>>()), Times.Once);
        }

        [Test]
        public void AssociateCheckListQuestionWithPackage_ShouldDoNothing_WhenQuestionListIsEmptyOrNull()
        {
            Sut.AssociateCheckListQuestionWithPackage(null, Guid.NewGuid());
            Sut.AssociateCheckListQuestionWithPackage(Enumerable.Empty<BusinessCheckListQuestion>(), Guid.NewGuid());

            _checkListQuestionToProductRepositoryMock.Verify(x => x.Insert(It.IsAny<IEnumerable<CheckListQuestionToProduct>>()), Times.Never);
        }

        [Test]
        public void AssociateCheckListQuestionWithPackage_ShouldThrowEntityNotFoundException_WhenReleaseWindowNotFound()
        {
            var questions = Builder<BusinessCheckListQuestion>.CreateListOfSize(5).Build();
            var releaseWindow = Builder<ReleaseWindow>.CreateNew().Build();
            _releaseWindowRepositoryMock.SetupEntities(Enumerable.Empty<ReleaseWindow>());

            var ex = Assert.Throws<EntityNotFoundException>(() => Sut.AssociateCheckListQuestionWithPackage(questions, releaseWindow.ExternalId));

            Assert.IsTrue(ex.Message.Contains(releaseWindow.ExternalId.ToString()), "Exception message should contain reference id");
            _checkListQuestionToProductRepositoryMock.Verify(x => x.Insert(It.IsAny<IEnumerable<CheckListQuestionToProduct>>()), Times.Never);
        }

        [Test]
        public void AssociateCheckListQuestionWithPackage_ShouldAssociateAllQuestionsWithReleaseWindowPackages_WhenQuestionsAreNotYetAssociated()
        {
            var dataQuestions = Builder<CheckListQuestion>.CreateListOfSize(5).Build();
            var releaseWindow = Builder<ReleaseWindow>.CreateNew()
                .With(x => x.ReleaseProducts, Builder<ReleaseProduct>.CreateListOfSize(2).Build())
                .With(x => x.CheckList, Enumerable.Empty<CheckList>())
                .Build();
            var questions = dataQuestions.Select(x => new BusinessCheckListQuestion
            {
                ExternalId = x.ExternalId,
                Question = x.Content,
                CheckListId = Guid.NewGuid()
            }).ToArray();

            _releaseWindowRepositoryMock.SetupEntities(new[] { releaseWindow });
            _checkListQuestionRepositoryMock.SetupEntities(dataQuestions);
            _checkListQuestionToProductRepositoryMock.SetupEntities(Enumerable.Empty<CheckListQuestionToProduct>());
            _checkListQuestionToProductRepositoryMock.Setup(x => x.Insert(It.Is<IEnumerable<CheckListQuestionToProduct>>(c =>
                c.Count() == 10
                && c.Count(z => z.CheckListQuestionId == dataQuestions.First().CheckListQuestionId) == 2
                && c.Count(z => z.ProductId == releaseWindow.ReleaseProducts.First().ProductId) == 5)));
            _checkListRepositoryMock.Setup(x => x.Insert(It.Is<IEnumerable<CheckList>>(c =>
                c.Count() == 5
                && c.Count(y => y.CheckListQuestionId == dataQuestions.First().CheckListQuestionId) == 1
                && c.Count(y => y.ExternalId == questions.First().CheckListId) == 1
                && c.Count(y => y.ReleaseWindowId == releaseWindow.ReleaseWindowId) == 5)));

            Sut.AssociateCheckListQuestionWithPackage(questions, releaseWindow.ExternalId);

            _checkListQuestionToProductRepositoryMock.Verify(x => x.Insert(It.IsAny<IEnumerable<CheckListQuestionToProduct>>()), Times.Once);
            _checkListRepositoryMock.Verify(x => x.Insert(It.IsAny<IEnumerable<CheckList>>()), Times.Once);
        }

        [Test]
        public void AssociateCheckListQuestionWithPackage_ShouldAssociateNotAllQuestionsWithReleaseWindowPackages_WhenSomeQuestionsAreAssociatedAlready()
        {
            var dataQuestions = Builder<CheckListQuestion>.CreateListOfSize(5).Build();
            var releaseWindow = Builder<ReleaseWindow>.CreateNew()
                .With(x => x.ReleaseProducts, Builder<ReleaseProduct>.CreateListOfSize(2).Build())
                .Build();
            var questions = dataQuestions.Select(x => new BusinessCheckListQuestion
            {
                ExternalId = x.ExternalId,
                Question = x.Content,
                CheckListId = Guid.NewGuid()
            }).ToArray();

            _releaseWindowRepositoryMock.SetupEntities(new[] { releaseWindow });
            _checkListQuestionRepositoryMock.SetupEntities(dataQuestions);
            _checkListQuestionToProductRepositoryMock.SetupEntities(new[]{ new CheckListQuestionToProduct
            {
                ProductId = releaseWindow.ReleaseProducts.First().ProductId,
                CheckListQuestionId = dataQuestions.First().CheckListQuestionId
            } });
            releaseWindow.CheckList = new[]{new CheckList
            {
                ReleaseWindowId = releaseWindow.ReleaseWindowId,
                CheckListQuestionId = dataQuestions.First().CheckListQuestionId
            } };
            _checkListQuestionToProductRepositoryMock.Setup(x => x.Insert(It.Is<IEnumerable<CheckListQuestionToProduct>>(c =>
                c.Count() == 9
                && c.Count(z => z.CheckListQuestionId == dataQuestions.First().CheckListQuestionId) == 1
                && c.Count(z => z.ProductId == releaseWindow.ReleaseProducts.First().ProductId) == 4)));
            _checkListRepositoryMock.Setup(x => x.Insert(It.Is<IEnumerable<CheckList>>(c =>
                c.Count() == 4
                && c.Count(y => y.CheckListQuestionId == dataQuestions.First().CheckListQuestionId) == 0
                && c.Count(y => y.ExternalId == questions.First().CheckListId) == 0
                && c.Count(y => y.CheckListQuestionId == dataQuestions.ElementAt(1).CheckListQuestionId) == 1
                && c.Count(y => y.ExternalId == questions.ElementAt(1).CheckListId) == 1
                && c.Count(y => y.ReleaseWindowId == releaseWindow.ReleaseWindowId) == 4)));

            Sut.AssociateCheckListQuestionWithPackage(questions, releaseWindow.ExternalId);

            _checkListQuestionToProductRepositoryMock.Verify(x => x.Insert(It.IsAny<IEnumerable<CheckListQuestionToProduct>>()), Times.Once);
            _checkListRepositoryMock.Verify(x => x.Insert(It.IsAny<IEnumerable<CheckList>>()), Times.Once);
        }

        [Test]
        public void RemoveCheckListQuestion_ShouldThrowEntityNotFoundException_WhenCheckListItemNotFound()
        {
            var checkListId = Guid.NewGuid();
            _checkListRepositoryMock.SetupEntities(Enumerable.Empty<CheckList>());

            var ex = Assert.Throws<EntityNotFoundException>(() => Sut.RemoveCheckListQuestion(checkListId));

            Assert.IsTrue(ex.Message.Contains(checkListId.ToString()), "Exception message should contain reference id");
            _checkListRepositoryMock.Verify(x => x.Delete(It.IsAny<CheckList>()), Times.Never);
        }


        [Test]
        public void RemoveCheckListQuestion_ShouldRemoveQuestionFromReleaseWindow_WhenInvoked()
        {
            var checkListItem = Builder<CheckList>.CreateNew().Build();
            _checkListRepositoryMock.SetupEntities(new[] { checkListItem });
            _checkListRepositoryMock.Setup(x => x.Delete(checkListItem));

            Sut.RemoveCheckListQuestion(checkListItem.ExternalId);

            _checkListRepositoryMock.Verify(x => x.Delete(It.IsAny<CheckList>()), Times.Once);
        }

        [Test]
        public void RemoveCheckListQuestionForPackage_ShouldThrowEntityNotFoundException_WhenCheckListItemNotFound()
        {
            var checkListId = Guid.NewGuid();
            _checkListRepositoryMock.SetupEntities(Enumerable.Empty<CheckList>());

            var ex = Assert.Throws<EntityNotFoundException>(() => Sut.RemoveCheckListQuestionForPackage(checkListId));

            Assert.IsTrue(ex.Message.Contains(checkListId.ToString()), "Exception message should contain reference id");
            _checkListRepositoryMock.Verify(x => x.Delete(It.IsAny<CheckList>()), Times.Never);
        }


        [Test]
        public void RemoveCheckListQuestionForPackage_ShouldRemoveQuestionFromPackageAndFromReleaseWindow_WhenInvoked()
        {
            var releaseWindow = Builder<ReleaseWindow>.CreateNew()
                .With(x => x.ReleaseProducts, Builder<ReleaseProduct>.CreateListOfSize(2).Build())
                .Build();
            var checkListItem = Builder<CheckList>.CreateNew()
                .With(x => x.ReleaseWindow = releaseWindow)
                .With(x => x.CheckListQuestion, Builder<CheckListQuestion>.CreateNew()
                    .With(q => q.CheckListQuestionsToProducts, Builder<CheckListQuestionToProduct>.CreateListOfSize(1)
                        .All()
                        .With(qp => qp.ProductId, releaseWindow.ReleaseProducts.First().ProductId)
                        .Build())
                    .Build())
                .Build();
            var checkListQuestionsToProducts =
                checkListItem.CheckListQuestion.CheckListQuestionsToProducts.First();
            _checkListRepositoryMock.SetupEntities(new[] { checkListItem });
            _checkListQuestionToProductRepositoryMock.Setup(x => x.GetByPrimaryKey(checkListQuestionsToProducts.CheckListQuestionsToProductsId))
                .Returns(checkListItem.CheckListQuestion.CheckListQuestionsToProducts.First());
            _checkListQuestionToProductRepositoryMock.Setup(x => x.Delete(checkListQuestionsToProducts));
            _checkListRepositoryMock.Setup(x => x.Delete(checkListItem));

            Sut.RemoveCheckListQuestionForPackage(checkListItem.ExternalId);

            _checkListQuestionToProductRepositoryMock.Verify(x => x.Delete(It.IsAny<CheckListQuestionToProduct>()), Times.Once);
            _checkListRepositoryMock.Verify(x => x.Delete(It.IsAny<CheckList>()), Times.Once);
        }

    }
}
