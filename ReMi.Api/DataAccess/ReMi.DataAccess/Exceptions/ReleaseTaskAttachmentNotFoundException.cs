using System;

namespace ReMi.DataAccess.Exceptions
{
	public class ReleaseTaskAttachmentNotFoundException : ApplicationException
	{
		public ReleaseTaskAttachmentNotFoundException(Guid releaseTaskAttachmentId)
            : base(FormatMessage(releaseTaskAttachmentId))
		{
		}

        public ReleaseTaskAttachmentNotFoundException(Guid releaseTaskAttachmentId, Exception innerException)
            : base(FormatMessage(releaseTaskAttachmentId), innerException)
		{
		}

        private static string FormatMessage(Guid releaseTaskAttachmentId)
		{
            return string.Format("Could not find release task attachment for id: {0}", releaseTaskAttachmentId);
		}
	}
}
