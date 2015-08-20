using ReMi.BusinessEntities.ReleaseCalendar;

namespace ReMi.BusinessLogic.ReleasePlan
{
    public interface IReleaseWindowHelper
    {
        bool IsMaintenance(ReleaseWindow releaseWindow);
    }
}
