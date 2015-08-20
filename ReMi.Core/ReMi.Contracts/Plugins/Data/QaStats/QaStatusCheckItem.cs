
namespace ReMi.Contracts.Plugins.Data.QaStats
{
    public class QaStatusCheckItem
    {
        public string Area { get; set; }
        public string MetricControl { get; set; }
        public string Status { get; set; }
        public string LastStatusUpdate { get; set; }
        public string Owner { get; set; }
        public string DetailsEvidence { get; set; }
        public string Comments { get; set; }
    }
}
