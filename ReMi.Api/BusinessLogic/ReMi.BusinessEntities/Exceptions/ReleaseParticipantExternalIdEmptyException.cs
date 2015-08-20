using System;
using ReMi.BusinessEntities.ReleasePlan;

namespace ReMi.BusinessEntities.Exceptions
{
    public class ReleaseParticipantExternalIdEmptyException : Exception
    {
        public ReleaseParticipantExternalIdEmptyException(ReleaseParticipant releaseParticipant)
            : base(FormatMessage(releaseParticipant))
        {
        }

        public ReleaseParticipantExternalIdEmptyException(ReleaseParticipant releaseParticipant, Exception innerException)
            : base(FormatMessage(releaseParticipant), innerException)
        {
        }

        private static string FormatMessage(ReleaseParticipant releaseParticipant)
        {
            return string.Format("Release participant external id cound't be found. Release participant: {0}",
                releaseParticipant);
        }
    }
}
