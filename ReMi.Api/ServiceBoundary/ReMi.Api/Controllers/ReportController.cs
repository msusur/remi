using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using ReMi.Common.WebApi;
using ReMi.Queries.Reports;

namespace ReMi.Api.Controllers
{
    [RoutePrefix("reports")]
    public class ReportController : ApiController
    {
        public IImplementQueryAction<ReportRequest, ReportResponse> ReportQuery { get; set; }
        public IImplementQueryAction<ReportsListRequest, ReportsListResponse> ReportListQuery { get; set; }

        [HttpGet]
        [Route("{reportName}")]
        public ReportResponse CreateReport(String reportName)
        {
            var request = new ReportRequest
            {
                ReportName = reportName,
                Parameters = Request.GetQueryNameValuePairs().ToDictionary(n=>n.Key, v=>v.Value)
            };

            return ReportQuery.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route]
        public ReportsListResponse GetReports()
        {
            var request = new ReportsListRequest();

            return ReportListQuery.Handle(ActionContext, request);
        }
    }
}
