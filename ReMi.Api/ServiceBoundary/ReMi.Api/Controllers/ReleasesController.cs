using ReMi.Common.WebApi;
using ReMi.Queries.ReleaseCalendar;
using ReMi.Queries.ReleaseExecution;
using ReMi.Queries.ReleasePlan;
using System;
using System.Web.Http;

namespace ReMi.Api.Controllers
{
    [RoutePrefix("releases")]
    public class ReleasesController : ApiController
    {
        public IImplementQueryAction<GetReleaseCalendarRequest, GetReleaseCalendarResponse>
            GetReleaseCalendarAction { get; set; }

        public IImplementQueryAction<GetReleaseContentInformationRequest, GetReleaseContentInformationResponse>
            GetReleaseContentInformationAction { get; set; }

        public IImplementQueryAction<GetTicketRiskRequest, GetTicketRiskResponse>
            GetTicketRiskAction { get; set; }

        public IImplementQueryAction<GetReleaseChangesRequest, GetReleaseChangesResponse> GetReleaseChagnesAction { get; set; }

        public IImplementQueryAction<GetUpcomingReleaseRequest, GetUpcomingReleaseResponse> GetUpcomingReleaseAction { get; set; }
        public IImplementQueryAction<GetCurrentReleaseRequest, GetCurrentReleaseResponse> GetCurrentReleaseAction { get; set; }

        public IImplementQueryAction<GetReleaseTasksRequest, GetReleaseTasksResponse> GetReleaseTasksAction { get; set; }

        public IImplementQueryAction<GetReleaseTaskRequest, GetReleaseTaskResponse> GetReleaseTaskAction { get; set; }
        public IImplementQueryAction<GetReleaseApproversRequest, GetReleaseApproversResponse> GetReleaseApproversAction { get; set; }
        public IImplementQueryAction<GetReleaseRequest, GetReleaseResponse> GetReleaseAction { get; set; }
        public IImplementQueryAction<GetNearReleasesRequest, GetNearReleasesResponse> GetNearReleasesAction { get; set; }
        public IImplementQueryAction<GetExpiredReleasesRequest, GetExpiredReleasesResponse> GetExpiredReleasesAction { get; set; }

        public IImplementQueryAction<GetReleaseEnumsRequest, GetReleaseEnumsResponse> GetReleaseEnumsAction { get; set; }

        public IImplementQueryAction<GetSignOffsRequest, GetSignOffsResponse> GetSignOffsAction { get; set; }

        public IImplementQueryAction<GetReleaseJobsRequest, GetReleaseJobsResponse>
            GetReleaseJobsAction { get; set; }

        [HttpGet]
        [Route("search/{startDay:datetime:regex(\\d{4}-\\d{2}-\\d{2})}/{endDay:datetime:regex(\\d{4}-\\d{2}-\\d{2})}")]
        public GetReleaseCalendarResponse GetReleaseCalendar(DateTime startDay, DateTime endDay)
        {
            var request = new GetReleaseCalendarRequest
            {
                StartDay = startDay,
                EndDay = endDay
            };

            return GetReleaseCalendarAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("releaseEnums")]
        public GetReleaseEnumsResponse GetReleaseEnums()
        {
            var request = new GetReleaseEnumsRequest();

            return GetReleaseEnumsAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("search/upcomingRelease/{product}")]
        public GetUpcomingReleaseResponse GetUpcomingRelease(string product)
        {
            var request = new GetUpcomingReleaseRequest
            {
                Product = product
            };

            return GetUpcomingReleaseAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("search/currentRelease/{product}")]
        public GetCurrentReleaseResponse GetCurrentRelease(string product)
        {
            var request = new GetCurrentReleaseRequest
            {
                Product = product
            };

            return GetCurrentReleaseAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("search/nearReleases/{product}")]
        public GetNearReleasesResponse GetNearReleases(string product)
        {
            var request = new GetNearReleasesRequest
            {
                Product = product
            };

            return GetNearReleasesAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("search/expired")]
        public GetExpiredReleasesResponse GetExpiredReleases()
        {
            var request = new GetExpiredReleasesRequest();

            return GetExpiredReleasesAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("{releaseWindowId:guid}")]
        public GetReleaseResponse GetRelease(Guid releaseWindowId)
        {
            var request = new GetReleaseRequest
            {
                ReleaseWindowId = releaseWindowId
            };

            return GetReleaseAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("{releaseWindowId}/content")]
        public GetReleaseContentInformationResponse GetReleaseContentInformation(Guid releaseWindowId)
        {
            var getReleaseContentInformationRequest = new GetReleaseContentInformationRequest
            {
                ReleaseWindowId = releaseWindowId
            };

            return GetReleaseContentInformationAction.Handle(ActionContext, getReleaseContentInformationRequest);
        }

        [HttpGet]
        [Route("ticketRisk")]
        public GetTicketRiskResponse GetTicketRisk()
        {
            var getTicketRiskRequest = new GetTicketRiskRequest();

            return GetTicketRiskAction.Handle(ActionContext, getTicketRiskRequest);
        }

        [HttpGet]
        [Route("{releaseWindowId}/changes")]
        public GetReleaseChangesResponse GetReleaseChanges(Guid releaseWindowId)
        {
            var releaseChangesRequest = new GetReleaseChangesRequest
            {
                ReleaseWindowId = releaseWindowId
            };

            return GetReleaseChagnesAction.Handle(ActionContext, releaseChangesRequest);
        }

        [HttpGet]
        [Route("{releaseWindowId:guid}/tasks")]
        public GetReleaseTasksResponse GetReleaseTasks(Guid releaseWindowId)
        {
            var request = new GetReleaseTasksRequest
            {
                ReleaseWindowId = releaseWindowId
            };

            return GetReleaseTasksAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("{releaseWindowId:guid}/approvers")]
        public GetReleaseApproversResponse GetReleaseApprovers(Guid releaseWindowId)
        {
            var request = new GetReleaseApproversRequest
            {
                ReleaseWindowId = releaseWindowId
            };

            return GetReleaseApproversAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("tasks/{releaseTaskId:guid}")]
        public GetReleaseTaskResponse GetReleaseTask(Guid releaseTaskId)
        {
            var request = new GetReleaseTaskRequest
            {
                ReleaseTaskId = releaseTaskId
            };

            return GetReleaseTaskAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("signers/{releaseWindowId:guid}")]
        public GetSignOffsResponse GetSignOffs(Guid releaseWindowId)
        {
            var request = new GetSignOffsRequest
            {
                ReleaseWindowId = releaseWindowId
            };

            return GetSignOffsAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("deploymentJobs/{releaseWindowId:guid}")]
        public GetReleaseJobsResponse GetDeploymentJobs(Guid releaseWindowId)
        {
            var request = new GetReleaseJobsRequest
            {
                ReleaseWindowId = releaseWindowId
            };

            return GetReleaseJobsAction.Handle(ActionContext, request);
        }
    }
}
