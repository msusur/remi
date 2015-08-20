using System.Collections.Generic;
using ReMi.BusinessEntities;
using ReMi.Common.Utils;

namespace ReMi.Queries.ReleasePlan
{
    public class GetReleaseTaskEnvironmentsResponse
    {
        public IEnumerable<EnumEntry> Environments { get; set; }

        public override string ToString()
        {
            return string.Format("[Environments={0}]", Environments.FormatElements());
        }
    }
}
