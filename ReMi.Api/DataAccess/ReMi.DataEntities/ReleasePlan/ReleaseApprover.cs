using System;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.DataEntities.Auth;
using ReMi.DataEntities.ReleaseCalendar;

namespace ReMi.DataEntities.ReleasePlan
{
    public class ReleaseApprover
    {
        #region scalar props

        public int ReleaseApproverId { get; set; }

        [Index(IsUnique = true)]
        public Guid ExternalId { get; set; }

        public int AccountId { get; set; }

        public int ReleaseWindowId { get; set; }

        public DateTime? ApprovedOn { get; set; }

        public DateTime CreatedOn { get; set; }

        public String Comment { get; set; }

        #endregion

        #region navigation props

        public virtual ReleaseWindow ReleaseWindow { get; set; }

        public virtual Account Account { get; set; }

        #endregion

        public override string ToString()
        {
            return string.Format("[ReleaseApproverId={0}, AccountId={1}, ReleaseWindowId={2}, ApprovedOn={3}, CreatedOn={4}, Comment={5}]",
                ReleaseApproverId, AccountId, ReleaseWindowId, ApprovedOn, CreatedOn, Comment);
        }
    }
}
