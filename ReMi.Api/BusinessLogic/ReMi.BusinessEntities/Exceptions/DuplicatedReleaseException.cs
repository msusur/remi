using System;
using ReMi.BusinessEntities.ReleaseCalendar;

namespace ReMi.BusinessEntities.Exceptions
{
	public class DuplicatedReleaseException : ApplicationException
	{
		public DuplicatedReleaseException(ReleaseWindow duplicatedRelease)
			: base(FormatMessage(duplicatedRelease))
		{
		}

		public DuplicatedReleaseException(ReleaseWindow duplicatedRelease, Exception innerException)
			: base(FormatMessage(duplicatedRelease), innerException)
		{
		}

		private static string FormatMessage(ReleaseWindow duplicatedRelease)
		{
			return string.Format("There is already a release booked with ExternalId={0}. ReleaseWindow: {1}",  duplicatedRelease.ExternalId, duplicatedRelease);
		}

		
	}
}
