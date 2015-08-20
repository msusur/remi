using ReMi.Contracts.Enums;

namespace ReMi.Common.Constants.ReleaseCalendar
{
    public enum ReleaseDecision
    {
        [EnumDescription("Undetermined")]
        Undetermined = 1,
        [EnumDescription("GO")]
        Go,
        [EnumDescription("NO GO")]
        NoGo
    }
}
