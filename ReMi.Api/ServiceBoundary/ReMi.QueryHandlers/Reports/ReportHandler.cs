using System;
using System.Collections.Generic;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.Reports;
using ReMi.Queries.Reports;

namespace ReMi.QueryHandlers.Reports
{
    public class ReportHandler : IHandleQuery<ReportRequest, ReportResponse>
    {
        public Func<IReportGateway> ReportGatewayFactory { get; set; }

        public ReportResponse Handle(ReportRequest request)
        {
            var formattedParameters = new Dictionary<String, object>();
            if(request.Parameters!=null)
            {
                DateTime temp;
                foreach (var parameter in request.Parameters)
                {
                    formattedParameters.Add(parameter.Key,
                        DateTime.TryParse(parameter.Value, out temp)
                            ? temp.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")
                            : parameter.Value);
                }
            }

            using (var gateway = ReportGatewayFactory())
            {
                return new ReportResponse
                {
                    Report = gateway.GetReport(request.ReportName, formattedParameters)
                };
            }
        }
    }
}
