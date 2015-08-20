using System;
using System.Collections.Generic;
using ReMi.Common.Utils;

namespace ReMi.BusinessEntities.Reports
{
    public class Report
    {
        public List<String> Headers { get; set; }
        public List<List<String>> Data { get; set; }

        public override string ToString()
        {
            return String.Format("Headers={0}, Data={1}", Headers.FormatElements(), Data.FormatElements());
        }
    }
}
