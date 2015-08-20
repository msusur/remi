using System;
using ReMi.BusinessEntities.Reports;

namespace ReMi.Queries.Reports
{
    public class ReportResponse
    {
        public Report Report { get; set; }

        public override string ToString()
        {
            return String.Format("[Data={0}]", Report);
        }
    }
}
