using ReMi.BusinessEntities.DeploymentTool;
using ReMi.Common.Utils;
using System.Collections.Generic;

namespace ReMi.Queries.ReleasePlan
{
    public class GetReleaseJobsResponse
    {
        public IEnumerable<ReleaseJob> ReleaseJobs { get; set; }

        public override string ToString()
        {
            return string.Format("[ReleaseJobs={0}]", ReleaseJobs.FormatElements());
        }
    }
}
