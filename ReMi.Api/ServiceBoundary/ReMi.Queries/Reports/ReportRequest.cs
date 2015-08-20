using System;
using System.Collections.Generic;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.Reports
{
    [Query("Get report", QueryGroup.Metrics)]
    public class ReportRequest : IQuery
    {
        public QueryContext Context { get; set; }
        public IDictionary<String, String> Parameters { get; set; }
        public String ReportName { get; set; }

        public override string ToString()
        {
            return String.Format("[Context={0}, ReportName={1}, Parameters={2}]", Context, ReportName,
                Parameters.FormatElements());
        }
    }
}
