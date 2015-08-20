using System;

namespace ReMi.DataAccess.Exceptions
{
	public class ReleaseWindowNotFoundException : ApplicationException
	{
		public ReleaseWindowNotFoundException(Guid releaseWindowId)
            : base(FormatMessage(releaseWindowId))
		{
		}

        public ReleaseWindowNotFoundException(Guid releaseWindowId, Exception innerException)
            : base(FormatMessage(releaseWindowId), innerException)
		{
		}

        private static string FormatMessage(Guid releaseWindowId)
		{
            return string.Format("Could not find release window for id: {0}", releaseWindowId);
		}
	}
}
