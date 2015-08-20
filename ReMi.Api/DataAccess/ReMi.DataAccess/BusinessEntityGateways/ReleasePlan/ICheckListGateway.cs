using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.ReleasePlan;

namespace ReMi.DataAccess.BusinessEntityGateways.ReleasePlan
{
    public interface ICheckListGateway : IDisposable
    {
        IEnumerable<CheckListItemView> GetCheckList(Guid releaseWindowId);
        List<CheckListQuestion> GetCheckListAdditionalQuestions(Guid releaseWindowId);
        List<String> GetCheckListQuestions();
        CheckListItemView GetCheckListItem(Guid checkListId);

        void Create(Guid releaseWindowId);
        void UpdateAnswer(CheckListItem checkListItem);
        void UpdateComment(CheckListItem checkListItem);
        void AddCheckListQuestions(IEnumerable<CheckListQuestion> questions, Guid releaseWindowId);
        void AssociateCheckListQuestionWithPackage(IEnumerable<CheckListQuestion> questions, Guid releaseWindowId);
        void RemoveCheckListQuestion(Guid checkListItemId);
        void RemoveCheckListQuestionForPackage(Guid checkListItemId);
    }
}
