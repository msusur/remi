using System;

namespace ReMi.BusinessEntities.Reports
{
    public class ReportParameter
    {
        public String Type { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }

        public override string ToString()
        {
            return String.Format("[Type={0}, Name={1}, Description={2}]", Type, Name, Description);
        }
    }
}
