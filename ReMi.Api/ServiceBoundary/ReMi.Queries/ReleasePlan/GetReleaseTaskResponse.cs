using ReMi.BusinessEntities.ReleasePlan;

namespace ReMi.Queries.ReleasePlan
{
    public class GetReleaseTaskResponse
    {
        public ReleaseTask ReleaseTask { get; set; }

        public override string ToString()
        {
            return string.Format("[ReleaseTask={0}]", ReleaseTask);
        }
    }
}
