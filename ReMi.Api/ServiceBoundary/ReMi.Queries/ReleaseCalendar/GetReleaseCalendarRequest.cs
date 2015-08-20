using System;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.ReleaseCalendar
{
    [Query("Get release calendar data", QueryGroup.ReleaseCalendar)]
    public class GetReleaseCalendarRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public DateTime StartDay { get; set; }

        public DateTime EndDay { get; set; }

        public override string ToString()
        {
            return string.Format("[StartDay = {0}, EndDay = {1}]", StartDay, EndDay);
        }
    }
}
