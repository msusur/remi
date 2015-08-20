using System.Collections.Generic;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Common.Utils;

namespace ReMi.Queries.ReleaseCalendar
{
    public class GetNearReleasesResponse
    {
        public IEnumerable<ReleaseWindow> ReleaseWindows { get; set; }

        public override string ToString()
        {
            return string.Format("[ReleaseWindows={0}]", ReleaseWindows.FormatElements());
        }
    }
}
