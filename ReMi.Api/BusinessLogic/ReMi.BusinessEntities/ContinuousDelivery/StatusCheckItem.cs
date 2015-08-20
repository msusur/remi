using System.Collections.Generic;
using ReMi.Common.Constants.ContinuousDelivery;

namespace ReMi.BusinessEntities.ContinuousDelivery
{
    public class StatusCheckItem
    {
        public string Area { get; set; }
        public string MetricControl { get; set; }
        public StatusType Status { get; set; }
        public IEnumerable<string> Comments { get; set; }
    }
}
