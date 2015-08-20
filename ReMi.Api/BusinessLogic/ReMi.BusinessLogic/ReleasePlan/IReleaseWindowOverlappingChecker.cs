using ReMi.BusinessEntities.ReleaseCalendar;

namespace ReMi.BusinessLogic.ReleasePlan
{
    public interface IReleaseWindowOverlappingChecker
    {
        ReleaseWindow FindOverlappedWindow(ReleaseWindow releaseWindow);
    }
}
