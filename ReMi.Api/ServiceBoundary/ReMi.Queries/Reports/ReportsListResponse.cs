using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.Reports;
using ReMi.Common.Utils;

namespace ReMi.Queries.Reports
{
    public class ReportsListResponse
    {
        public IEnumerable<ReportDescription> ReportList { get; set; }

        public override string ToString()
        {
            return String.Format("[ReportList={0}]", ReportList.FormatElements());
        }
    }
}
