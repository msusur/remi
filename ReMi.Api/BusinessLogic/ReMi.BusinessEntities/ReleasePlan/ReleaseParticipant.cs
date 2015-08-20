using System;
using ReMi.BusinessEntities.Auth;

namespace ReMi.BusinessEntities.ReleasePlan
{
    public class ReleaseParticipant
    {
        public Guid ReleaseWindowId { get; set; }
        public Account Account { get; set; }
        public Guid ReleaseParticipantId { get; set; }
        public bool IsParticipationConfirmed { get; set; }

        public override string ToString()
        {
            return
                String.Format(
                    "ReleaseWindowId={0}, Account={1}, ReleaseParticipantId={2}, IsParticipationConfirmed={3}",
                    ReleaseWindowId, Account, ReleaseParticipantId, IsParticipationConfirmed);
        }
    }
}
