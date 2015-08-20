using System.Collections.Generic;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Common.Utils;

namespace ReMi.Queries.ReleasePlan
{
    public class GetReleaseTasksResponse
    {
        public IEnumerable<ReleaseTaskView> ReleaseTasks { get; set; }

        public override string ToString()
        {
            return string.Format("[ReleaseTasks = {0}]", ReleaseTasks.FormatElements());
        }
    }
}
