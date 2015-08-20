
using System;
using ReMi.DataEntities.Auth;
using ReMi.DataEntities.ReleaseCalendar;

namespace ReMi.DataEntities
{
    public class ReleaseParticipant
    {
        #region scalar props

        public int ReleaseParticipantId { get; set; }

        public int AccountId { get; set; }

        public int ReleaseWindowId { get; set; }
        
        public Guid ExternalId { get; set; }

        public DateTime? ApprovedOn { get; set; }

        #endregion

        #region navigation props

        public virtual ReleaseWindow ReleaseWindow { get; set; }

        public virtual Account Account { get; set; }

        #endregion

        public override string ToString()
        {
            return string.Format("[AccountId = {0}, ReleaseWindowId = {1}]", AccountId, ReleaseWindowId);
        }
    }
}
