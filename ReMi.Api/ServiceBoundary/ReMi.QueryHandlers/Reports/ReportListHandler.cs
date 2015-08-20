using System;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.Reports;
using ReMi.Queries.Reports;

namespace ReMi.QueryHandlers.Reports
{
    public class ReportListHandler : IHandleQuery<ReportsListRequest, ReportsListResponse>
    {
        public Func<IReportGateway> ReportGatewayFactory { get; set; }

        public ReportsListResponse Handle(ReportsListRequest request)
        {
            using (var gateway = ReportGatewayFactory())
            {
                return new ReportsListResponse
                {
                    ReportList = gateway.GetReportDescriptions()
                };
            }
        }
    }
}
