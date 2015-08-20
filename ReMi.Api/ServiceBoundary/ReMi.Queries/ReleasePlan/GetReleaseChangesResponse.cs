using ReMi.Common.Utils;
using ReMi.Contracts.Plugins.Data.SourceControl;
using System.Collections.Generic;
using System.Linq;

namespace ReMi.Queries.ReleasePlan
{
    public class GetReleaseChangesResponse
    {
        public IEnumerable<ReleaseRepository> Repositories { get; set; }
        public IEnumerable<SourceControlChange> Changes { get; set; }

        public override string ToString()
        {
            return string.Format("[Changes={0}]",
                Changes.Select(o => o.Description).FormatElements());
        }
    }
}
