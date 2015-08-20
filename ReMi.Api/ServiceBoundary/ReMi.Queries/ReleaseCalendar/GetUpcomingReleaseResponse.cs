using ReMi.BusinessEntities.ReleaseCalendar;

namespace ReMi.Queries.ReleaseCalendar
{
    public class GetUpcomingReleaseResponse
    {
        public ReleaseWindow ReleaseWindow { get; set; }

        public override string ToString()
        {
            return string.Format("[ReleaseWindow={0}]", ReleaseWindow);
        }
    }
}
