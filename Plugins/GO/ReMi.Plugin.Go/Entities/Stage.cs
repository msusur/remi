
namespace ReMi.Plugin.Go.Entities
{
    public class Stage
    {
        public string StageLocator { get; set; }
        public string StageStatus { get; set; }
        public string StageName { get; set; }
        public string StageId { get; set; }

        public override string ToString()
        {
            return string.Format("[StageName={0}, StageId={1}, StageStatus={2}, StageLocator={3}]",
                StageName, StageId, StageStatus, StageLocator);
        }
    }
}
