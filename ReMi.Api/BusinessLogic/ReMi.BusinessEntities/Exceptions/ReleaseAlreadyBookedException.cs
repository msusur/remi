using System;
using ReMi.BusinessEntities.ReleaseCalendar;

namespace ReMi.BusinessEntities.Exceptions
{
    public class ReleaseAlreadyBookedException : ApplicationException
    {
        public ReleaseAlreadyBookedException(DateTime startTime)
            : this(null, startTime)
        {
        }

        public ReleaseAlreadyBookedException(DateTime startTime, Exception innerException)
            : this(null, startTime, innerException)
        {
        }

        public ReleaseAlreadyBookedException(ReleaseWindow overlappingRelease, DateTime startTime)
            : base(FormatMessage(overlappingRelease, startTime))
        {
        }

        public ReleaseAlreadyBookedException(ReleaseWindow overlappingRelease, DateTime startTime, Exception innerException)
            : base(FormatMessage(overlappingRelease, startTime), innerException)
        {
        }

        private static string FormatMessage(ReleaseWindow overlappingRelease, DateTime startTime)
        {
            if (overlappingRelease == null)
            {
                return FormatTimeRangeMessage(startTime);
            }

            return string.Format("{0}. Overlapping release: {1}", FormatTimeRangeMessage(startTime), overlappingRelease);
        }

        private static string FormatTimeRangeMessage(DateTime startTime)
        {
            return string.Format("A release has already been booked with start time {0}", startTime);
        }
    }
}
