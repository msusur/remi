using System;
using ReMi.BusinessEntities.ReleaseCalendar;

namespace ReMi.BusinessEntities.Exceptions
{
    public class ReleaseNotFoundException : ApplicationException
    {
        public ReleaseNotFoundException(ReleaseWindow releaseWindow)
            : base(FormatMessage(releaseWindow))
        {
        }

        public ReleaseNotFoundException(Guid releaseWindowId)
            : base(FormatMessage(releaseWindowId))
        {
        }

        public ReleaseNotFoundException(ReleaseWindow releaseWindow, Exception innerException)
            : base(FormatMessage(releaseWindow), innerException)
        {
        }

        private static string FormatMessage(ReleaseWindow releaseWindow)
        {
            return string.Format("Release cound't be found. ReleaseWindow: {0}", releaseWindow);
        }

        private static string FormatMessage(Guid releaseWindowId)
        {
            return String.Format("Release cound't be found. ReleaseWindowId: {0}", releaseWindowId);
        }
    }
}
