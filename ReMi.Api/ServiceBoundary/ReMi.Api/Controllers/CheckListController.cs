using ReMi.Common.WebApi;
using ReMi.Queries.ReleasePlan;
using System;
using System.Web.Http;

namespace ReMi.Api.Controllers
{
    [RoutePrefix("checkList")]
    public class CheckListController : ApiController
    {
        public IImplementQueryAction<GetCheckListRequest, GetCheckListResponse> GetCheckListAction { get; set; }
        public IImplementQueryAction<CheckListAdditionalQuestionRequest, CheckListAdditionalQuestionResponse> GetAdditionalQuestionAction { get; set; }
       

        [HttpGet]
        [Route("{releaseWindowId:Guid}")]
        public GetCheckListResponse GetCheckList(Guid releaseWindowId)
        {
            var request = new GetCheckListRequest
            {
                ReleaseWindowId = releaseWindowId
            };

            return GetCheckListAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("additionalQuestions/{releaseWindowId:Guid}")]
        public CheckListAdditionalQuestionResponse GetAdditionalQuestions(Guid releaseWindowId)
        {
            var request = new CheckListAdditionalQuestionRequest
            {
                ReleaseWindowId = releaseWindowId
            };

            return GetAdditionalQuestionAction.Handle(ActionContext, request);
        }
    }
}
