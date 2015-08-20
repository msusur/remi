using ReMi.BusinessEntities.ReleaseCalendar;
using System.Collections.Generic;

namespace ReMi.Queries.ReleaseCalendar
{
    public class GetReleaseEnumsResponse
    {
        public IEnumerable<ReleaseTypeDescription> ReleaseTypes { get; set; }

        public override string ToString()
        {
            return string.Format("[ReleaseTypes = {0}]", ReleaseTypes);
        }
    }
}
