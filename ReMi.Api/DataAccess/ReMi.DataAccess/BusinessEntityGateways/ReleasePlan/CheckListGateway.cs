using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Common.Utils;
using ReMi.Common.Utils.Repository;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.ReleaseCalendar;
using ReMi.DataEntities.ReleasePlan;
using System;
using System.Collections.Generic;
using System.Linq;
using BusinessCheckListQuestion = ReMi.BusinessEntities.ReleasePlan.CheckListQuestion;
using CheckListQuestion = ReMi.DataEntities.ReleasePlan.CheckListQuestion;

namespace ReMi.DataAccess.BusinessEntityGateways.ReleasePlan
{
    public class CheckListGateway : BaseGateway, ICheckListGateway
    {
        public IRepository<CheckListQuestion> CheckListQuestionRepository { get; set; }
        public IRepository<CheckList> CheckListRepository { get; set; }
        public IRepository<ReleaseWindow> ReleaseWindowRepository { get; set; }
        public IRepository<CheckListQuestionToProduct> CheckListQuestionToProductRepository { get; set; }

        public override void OnDisposing()
        {
            CheckListQuestionRepository.Dispose();
            CheckListQuestionToProductRepository.Dispose();
            CheckListRepository.Dispose();
            ReleaseWindowRepository.Dispose();

            base.OnDisposing();
        }

        public IEnumerable<CheckListItemView> GetCheckList(Guid releaseWindowId)
        {
            var result = ReleaseWindowRepository
                .GetAllSatisfiedBy(r => r.ExternalId == releaseWindowId)
                .FirstOrDefault();
            return result != null ? result.CheckList.Select(x => new CheckListItemView
            {
                Checked = x.Checked,
                CheckListQuestion = x.CheckListQuestion.Content,
                Comment = x.Comment,
                ReleaseWindowId = x.ReleaseWindow.ExternalId,
                ExternalId = x.ExternalId,
                LastChangedBy = x.LastChangedBy
            }).ToList() : null;
        }

        public CheckListItemView GetCheckListItem(Guid checkListId)
        {
            var checkListItem = CheckListRepository.GetSatisfiedBy(c => c.ExternalId == checkListId);
            return new CheckListItemView
            {
                Checked = checkListItem.Checked,
                CheckListQuestion = checkListItem.CheckListQuestion.Content,
                Comment = checkListItem.Comment,
                ReleaseWindowId = checkListItem.ReleaseWindow.ExternalId,
                ExternalId = checkListItem.ExternalId,
                LastChangedBy = checkListItem.LastChangedBy
            };
        }

        public List<BusinessCheckListQuestion> GetCheckListAdditionalQuestions(Guid releaseWindowId)
        {
            var questions =
                CheckListQuestionRepository.GetAllSatisfiedBy(
                    c => c.CheckLists.All(ch => ch.ReleaseWindow.ExternalId != releaseWindowId))
                    .Select(c => new BusinessCheckListQuestion { ExternalId = c.ExternalId, Question = c.Content })
                    .ToList();

            return questions;
        }

        public List<string> GetCheckListQuestions()
        {
            return CheckListQuestionRepository.Entities.Select(c => c.Content).ToList();
        }

        public void Create(Guid releaseWindowId)
        {
            var releaseWindow = ReleaseWindowRepository
                .GetAllSatisfiedBy(r => r.ExternalId == releaseWindowId)
                .FirstOrDefault();

            if (releaseWindow != null && releaseWindow.ReleaseProducts.Any() && (releaseWindow.CheckList == null || !releaseWindow.CheckList.Any()))
            {
                var localProducts = releaseWindow.ReleaseProducts.Select(x => x.Product.Description).ToArray();

                var productQuestions =
                    CheckListQuestionToProductRepository.GetAllSatisfiedBy(c => localProducts.Contains(c.Product.Description)).ToList();

                var questions = CheckListQuestionRepository.Entities.ToList();

                var checkListQuestions = questions
                    .Where(c =>
                        productQuestions.Any(q => q.CheckListQuestionId == c.CheckListQuestionId))
                    .ToList();

                foreach (var checkListQuestion in checkListQuestions)
                {
                    var checkListItem = new CheckList
                    {
                        ExternalId = Guid.NewGuid(),
                        ReleaseWindowId = releaseWindow.ReleaseWindowId,
                        CheckListQuestionId = checkListQuestion.CheckListQuestionId
                    };

                    CheckListRepository.Insert(checkListItem);
                }
            }
        }

        public void UpdateAnswer(CheckListItem checkListItem)
        {
            var checkListToUpdate = CheckListRepository.GetSatisfiedBy(x => x.ExternalId == checkListItem.ExternalId);
            checkListToUpdate.Checked = checkListItem.Checked;
            checkListToUpdate.LastChangedBy = checkListItem.LastChangedBy;

            CheckListRepository.Update(checkListToUpdate);
        }

        public void UpdateComment(CheckListItem checkListItem)
        {
            var checkListToUpdate = CheckListRepository.GetSatisfiedBy(x => x.ExternalId == checkListItem.ExternalId);
            checkListToUpdate.Comment = checkListItem.Comment;
            checkListToUpdate.LastChangedBy = checkListItem.LastChangedBy;

            CheckListRepository.Update(checkListToUpdate);
        }

        public void AddCheckListQuestions(IEnumerable<BusinessCheckListQuestion> questions, Guid releaseWindowId)
        {
            if (questions.IsNullOrEmpty())
                return;

            CheckListQuestionRepository.Insert(questions
                .Select(x => new CheckListQuestion
                {
                    Content = x.Question,
                    ExternalId = x.ExternalId
                }));

            AssociateCheckListQuestionWithPackage(questions, releaseWindowId);
        }

        public void AssociateCheckListQuestionWithPackage(IEnumerable<BusinessCheckListQuestion> questions, Guid releaseWindowId)
        {
            if (questions.IsNullOrEmpty())
                return;

            var window = ReleaseWindowRepository.GetSatisfiedBy(w => w.ExternalId == releaseWindowId);
            if (window == null)
                throw new EntityNotFoundException(typeof(ReleaseWindow), releaseWindowId);

            var packageIds = window.ReleaseProducts.Select(x => x.ProductId).ToArray();

            var questionExternalIds = questions.Select(x => x.ExternalId).ToArray();

            var dataQuestions = CheckListQuestionRepository
                .GetAllSatisfiedBy(c => questionExternalIds.Any(id => id == c.ExternalId))
                .ToArray();

            var questionsToAdd = packageIds.SelectMany(x => dataQuestions, (id, q) => new {PackageId = id, Question = q})
                .Where(x => !CheckListQuestionToProductRepository.Entities
                    .Any(c => c.ProductId == x.PackageId && c.CheckListQuestionId == x.Question.CheckListQuestionId))
                .Select(x => new CheckListQuestionToProduct
                {
                    ProductId = x.PackageId,
                    CheckListQuestionId = x.Question.CheckListQuestionId,
                })
                .ToArray();

            CheckListQuestionToProductRepository.Insert(questionsToAdd);

            var checkListItemsToAdd = (from bq in questions
                join dq in dataQuestions on bq.ExternalId equals dq.ExternalId
                where window.CheckList.All(x => x.CheckListQuestionId != dq.CheckListQuestionId)
                select new CheckList
                {
                    ExternalId = bq.CheckListId,
                    ReleaseWindowId = window.ReleaseWindowId,
                    CheckListQuestionId = dq.CheckListQuestionId,
                }).ToArray();

            CheckListRepository.Insert(checkListItemsToAdd);
        }

        public void RemoveCheckListQuestion(Guid checkListItemId)
        {
            var checkListItem = CheckListRepository.GetSatisfiedBy(
                    q => q.ExternalId == checkListItemId);

            if (checkListItem == null)
                throw new EntityNotFoundException(typeof(CheckList), checkListItemId);

            CheckListRepository.Delete(checkListItem);
        }

        public void RemoveCheckListQuestionForPackage(Guid checkListItemId)
        {
            var checkListItem = CheckListRepository.GetSatisfiedBy(
                    q => q.ExternalId == checkListItemId);

            if (checkListItem == null)
                throw new EntityNotFoundException(typeof(CheckList), checkListItemId);

            var window = checkListItem.ReleaseWindow;

            var windowPackageIds = window.ReleaseProducts.Select(x => x.ProductId).ToArray();

            var questionToProducts = checkListItem.CheckListQuestion.CheckListQuestionsToProducts
                .Where(x => windowPackageIds.Any(p => p == x.ProductId))
                .ToList();

            questionToProducts.ForEach(x => CheckListQuestionToProductRepository.Delete(x.CheckListQuestionsToProductsId));
            
            CheckListRepository.Delete(checkListItem);
        }
    }
}
