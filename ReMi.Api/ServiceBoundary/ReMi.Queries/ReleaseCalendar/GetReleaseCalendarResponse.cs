using System.Collections.Generic;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Common.Utils;

namespace ReMi.Queries.ReleaseCalendar
{
    public class GetReleaseCalendarResponse
    {
        public IEnumerable<ReleaseWindowView> ReleaseWindows { get; set; }

        public override string ToString()
        {
            return string.Format("[ReleaseWindows = {0}]", ReleaseWindows.FormatElements());
        }
    }
}
