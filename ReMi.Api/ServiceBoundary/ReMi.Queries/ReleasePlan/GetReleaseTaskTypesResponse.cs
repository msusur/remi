using System.Collections.Generic;
using ReMi.BusinessEntities;
using ReMi.Common.Utils;

namespace ReMi.Queries.ReleasePlan
{
    public class GetReleaseTaskTypesResponse
    {
        public IEnumerable<EnumEntry> ReleaseTaskTypes { get; set; }

        public override string ToString()
        {
            return string.Format("[ReleaseTaskTypes = {0}]", ReleaseTaskTypes.FormatElements());
        }
    }
}
