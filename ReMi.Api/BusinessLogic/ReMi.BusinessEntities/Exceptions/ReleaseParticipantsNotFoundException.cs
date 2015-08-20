using System;

namespace ReMi.BusinessEntities.Exceptions
{
    public class ReleaseParticipantsNotFoundException : Exception
    {
        public ReleaseParticipantsNotFoundException(Guid releaseWindowId)
            : base(FormatMessage(releaseWindowId))
        {
        }

        public ReleaseParticipantsNotFoundException(Guid releaseWindowId, Exception innerException)
            : base(FormatMessage(releaseWindowId), innerException)
        {
        }

        private static string FormatMessage(Guid releaseWindowId)
        {
            return string.Format("Release participants cound't be found. ReleaseWindowId: {0}", releaseWindowId);
        }
    }
}
