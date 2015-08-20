using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Common.Constants;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Utils.Enums;

namespace ReMi.BusinessLogic.ReleasePlan
{
    public class ReleaseWindowHelper : IReleaseWindowHelper
    {
        public bool IsMaintenance(ReleaseWindow releaseWindow)
        {
            var releaseType = EnumDescriptionHelper.GetEnumDescription<ReleaseType, ReleaseTypeDescription>(releaseWindow.ReleaseType);
            return releaseType.IsMaintenance;
        }
    }
}
