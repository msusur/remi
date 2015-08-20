using System.Collections.Generic;
using ReMi.Common.Utils;

namespace ReMi.Queries.ReleaseParticipant
{
    public class GetReleaseParticipantResponse
    {
        public IEnumerable<BusinessEntities.ReleasePlan.ReleaseParticipant> Participants { get; set; }

        public override string ToString()
        {
            return Participants.FormatElements();
        }
    }
}
