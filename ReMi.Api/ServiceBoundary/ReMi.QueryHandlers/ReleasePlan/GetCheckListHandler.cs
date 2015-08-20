using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Queries.ReleasePlan;

namespace ReMi.QueryHandlers.ReleasePlan
{
    public class GetCheckListHandler : IHandleQuery<GetCheckListRequest, GetCheckListResponse>, IHandleQuery<CheckListAdditionalQuestionRequest, CheckListAdditionalQuestionResponse>
    {
        public Func<ICheckListGateway> CheckListGatewayFactory { get; set; }

        public GetCheckListResponse Handle(GetCheckListRequest request)
        {
            IEnumerable<CheckListItemView> result;
            using (var checkListGateway = CheckListGatewayFactory())
            {
                result = checkListGateway.GetCheckList(request.ReleaseWindowId);
            }

            return new GetCheckListResponse
            {
                CheckList = result
            };
        }

        public CheckListAdditionalQuestionResponse Handle(CheckListAdditionalQuestionRequest request)
        {
            using (var gateway = CheckListGatewayFactory())
            {
                return new CheckListAdditionalQuestionResponse
                {
                    Questions = gateway.GetCheckListAdditionalQuestions(request.ReleaseWindowId)
                };
            }

        }
    }
}
