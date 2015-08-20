using System.Collections.Generic;
using ReMi.BusinessEntities.ReleaseCalendar;

namespace ReMi.Queries.Configuration
{
    public class GetReleaseTrackResponse
    {
        public IEnumerable<ReleaseTrackDescription> ReleaseTrack { get; set; }

        public override string ToString()
        {
            return string.Format("[ReleaseTrack = {0}]", ReleaseTrack);
        }
    }
}
