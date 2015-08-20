using System;

namespace ReMi.DataAccess.Exceptions
{
	public class ReleaseTaskNotFoundException : ApplicationException
	{
		public ReleaseTaskNotFoundException(Guid releaseTaskId)
            : base(FormatMessage(releaseTaskId))
		{
		}

        public ReleaseTaskNotFoundException(Guid releaseTaskId, Exception innerException)
            : base(FormatMessage(releaseTaskId), innerException)
		{
		}

        private static string FormatMessage(Guid releaseTaskId)
		{
            return string.Format("Could not find release task for id: {0}", releaseTaskId);
		}
	}
}
