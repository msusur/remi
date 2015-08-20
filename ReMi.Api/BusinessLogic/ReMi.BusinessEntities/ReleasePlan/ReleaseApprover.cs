using System;
using ReMi.BusinessEntities.Auth;

namespace ReMi.BusinessEntities.ReleasePlan
{
    public class ReleaseApprover
    {
        public Guid ExternalId { get; set; }

        public Guid ReleaseWindowId { get; set; }

        public Account Account { get; set; }

        public DateTime? ApprovedOn { get; set; }

        public String Comment { get; set; }

        public override string ToString()
        {
            return
                String.Format(
                    "[ExternalId={0}, ReleaseWindowId={1}, ApprovedOn={2}, Account={3}, Comment={4}]",
                    ExternalId, ReleaseWindowId, ApprovedOn, Account, Comment);
        }
    }
}
