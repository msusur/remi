using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.Reports;

namespace ReMi.DataAccess.BusinessEntityGateways.Reports
{
    public interface IReportGateway : IDisposable
    {
        Report GetReport(String reportName, IDictionary<String, object> parameters);
        IEnumerable<ReportDescription> GetReportDescriptions();
    }
}
