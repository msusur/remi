using System;
using ReMi.Common.Constants;
using ReMi.Common.Constants.ReleaseExecution;
using ReMi.Common.Utils.Enums;

namespace ReMi.BusinessEntities.Metrics
{
    public class Metric
    {
        public Guid ExternalId { get; set; }
        public DateTime? ExecutedOn { get; set; }
        public MetricType MetricType { get; set; }
        public int Order { get; set; }

        public string MetricTypeName { get { return EnumDescriptionHelper.GetDescription(MetricType); } }

        public override string ToString()
        {
            return
                String.Format(
                    "[ExternalId={0}, ExecutedOn={1}, MetricType={2}, Order={3}]",
                    ExternalId, ExecutedOn, MetricType, Order);
        }
    }
}
