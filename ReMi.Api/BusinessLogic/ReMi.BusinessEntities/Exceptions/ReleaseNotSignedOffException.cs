using System;
using ReMi.BusinessEntities.ReleaseCalendar;

namespace ReMi.BusinessEntities.Exceptions
{
    public class ReleaseNotSignedOffException : ApplicationException
    {
        public ReleaseNotSignedOffException(ReleaseWindow releaseWindow)
            : base(FormatMessage(releaseWindow))
        {
        }

        public ReleaseNotSignedOffException(Guid releaseWindowId)
            : base(FormatMessage(releaseWindowId))
        {
        }

        public ReleaseNotSignedOffException(ReleaseWindow releaseWindow, Exception innerException)
            : base(FormatMessage(releaseWindow), innerException)
        {
        }

        private static string FormatMessage(ReleaseWindow releaseWindow)
        {
            return string.Format("Release is not signed off. ReleaseWindow: {0}", releaseWindow);
        }

        private static string FormatMessage(Guid releaseWindowId)
        {
            return String.Format("Release is not signed off. ReleaseWindowId: {0}", releaseWindowId);
        }
    }
}
