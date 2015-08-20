using System;

namespace ReMi.BusinessEntities.ReleaseCalendar
{
	public class ReleaseCalendarFilter
	{
		public DateTime StartDay { get; set; }

		public DateTime EndDay { get; set; }

		public override string ToString()
		{
			return string.Format("[StartDay = {0}, EndDay = {1}]", StartDay, EndDay);
		}
	}
}
