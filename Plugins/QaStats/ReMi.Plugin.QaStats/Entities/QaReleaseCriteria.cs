
namespace ReMi.Plugin.QaStats.Entities
{
    public class QaReleaseCriteria
    {
        public string Area { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string LastStatusUpdate { get; set; }
        public string Owner { get; set; }
        public string DetailsEvidence { get; set; }
        public string[] Comments { get; set; }
    }
}
