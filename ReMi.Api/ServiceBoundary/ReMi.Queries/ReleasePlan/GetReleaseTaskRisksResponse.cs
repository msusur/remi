using System.Collections.Generic;
using ReMi.BusinessEntities;
using ReMi.Common.Utils;

namespace ReMi.Queries.ReleasePlan
{
    public class GetReleaseTaskRisksResponse
    {
        public IEnumerable<EnumEntry> Risks { get; set; }

        public override string ToString()
        {
            return string.Format("[Risks={0}]", Risks.FormatElements());
        }
    }
}
