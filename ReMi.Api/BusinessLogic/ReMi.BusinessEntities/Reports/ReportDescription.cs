using System;
using System.Collections.Generic;
using ReMi.Common.Utils;

namespace ReMi.BusinessEntities.Reports
{
    public class ReportDescription
    {
        public String ReportName { get; set; }
        public String ReportCreator { get; set; }
        public List<ReportParameter> ReportParameters { get; set; }

        public override string ToString()
        {
            return String.Format("[ReportName={0}, ReportCreator={1}, ReportParameters={2}]", ReportName, ReportCreator, ReportParameters.FormatElements());
        }
    }
}
