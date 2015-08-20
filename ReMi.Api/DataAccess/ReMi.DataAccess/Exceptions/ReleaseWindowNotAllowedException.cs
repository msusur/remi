using System;

namespace ReMi.DataAccess.Exceptions
{
	public class ReleaseWindowNotAllowedException : ApplicationException
	{
		public ReleaseWindowNotAllowedException(Guid releaseWindowId)
            : base(FormatMessage(releaseWindowId, string.Empty))
		{
		}
        public ReleaseWindowNotAllowedException(Guid releaseWindowId, string reason)
            : base(FormatMessage(releaseWindowId, reason))
        {
        }

        public ReleaseWindowNotAllowedException(Guid releaseWindowId, Exception innerException)
            : base(FormatMessage(releaseWindowId, string.Empty), innerException)
		{
		}
        public ReleaseWindowNotAllowedException(Guid releaseWindowId, string reason, Exception innerException)
            : base(FormatMessage(releaseWindowId, reason), innerException)
        {
        }

        private static string FormatMessage(Guid releaseWindowId, string reason)
		{
            return string.Format("Release window not allowed. Reason={1}, Id={0}", releaseWindowId, reason);
		}
	}
}
