using System.Collections.Generic;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Common.Utils;

namespace ReMi.Queries.ReleasePlan
{
    public class GetReleaseApproversResponse
    {
        public IEnumerable<ReleaseApprover> Approvers { get; set; }

        public override string ToString()
        {
            return string.Format("[Approvers={0}]", Approvers.FormatElements());
        }
    }
}
