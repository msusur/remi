using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ReMi.BusinessEntities.Reports;
using ReMi.Common.Utils.Repository;
using ReMi.DataAccess.Exceptions;
using ReMi.DataAccess.Helpers;
using DataReportDescription= ReMi.DataEntities.Reports.ReportDescription;
using ReportDescription = ReMi.BusinessEntities.Reports.ReportDescription;

namespace ReMi.DataAccess.BusinessEntityGateways.Reports
{
    public class ReportGateway : BaseGateway, IReportGateway
    {
        public IDatabaseAdapter DatabaseAdapter { get; set; }

        public IRepository<DataReportDescription> ReportDesriptionRepository { get; set; }

        public Report GetReport(string procedureName, IDictionary<string, object> parameters)
        {
            var report = ReportDesriptionRepository.GetSatisfiedBy(x => x.ProcedureName == procedureName);

            if (report == null)
            {
                throw new EntityNotFoundException("Report description", procedureName);
            }

            if (report.ReportColumns == null || !report.ReportColumns.Any())
            {
                throw new EntityNotFoundException("Report column", procedureName);
            }
            
            var procedureParams = new Dictionary<String, object>();
            foreach (var parameter in parameters)
            {
                var type = report.ReportParameters.First(x => x.Name == parameter.Key).Type;
                procedureParams.Add("@" + parameter.Key,
                    type.ToLower() == "report.packages" ? AdaptPackageDataTable(parameter.Value.ToString()) : parameter.Value);
            }

            var result = RunProcedure(String.Format("Report.{0}", report.ProcedureName), report.ReportColumns.Count,
                procedureParams);

            var createdReport = new Report
            {
                Headers =
                    report.ReportColumns.OrderBy(r => r.Order)
                        .Select(r => r.Name).ToList(),
                Data = result
            };
            
            return createdReport;
        }

        public IEnumerable<ReportDescription> GetReportDescriptions()
        {
            var resultList = new List<ReportDescription>();

            var existingReports = ReportDesriptionRepository.Entities.ToList();

            foreach (var report in existingReports)
            {
                resultList.Add(new ReportDescription
                {
                    ReportName = report.Name,
                    ReportParameters = report.ReportParameters.Select(r=>new BusinessEntities.Reports.ReportParameter
                    {
                        Name = r.Name,
                        Type = r.Type,
                        Description = r.Description
                    }).ToList(),
                    ReportCreator = report.ProcedureName
                });
            }

            return resultList;
        }

        private DataTable AdaptPackageDataTable(string param)
        {
            var rows = param.Split(',');
            var table = new DataTable();
            table.Columns.Add("ExternalId");
            foreach (var row in rows)
            {
                var newRow = table.NewRow();
                newRow["ExternalId"] = row;
                table.Rows.Add(newRow);
            }
            return table;
        }

        private List<List<String>> RunProcedure
            (string name, int columns, IDictionary<String, object> parameters = null)
        {
            return DatabaseAdapter.RunStoredProcedure(name, columns, parameters);
        }

        public override void OnDisposing()
        {
            DatabaseAdapter.Dispose();
            ReportDesriptionRepository.Dispose();

            base.OnDisposing();
        }
    }
}
