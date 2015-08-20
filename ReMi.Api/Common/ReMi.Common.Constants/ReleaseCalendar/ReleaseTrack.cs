using ReMi.Contracts.Enums;

namespace ReMi.Common.Constants.ReleaseCalendar
{
    public enum ReleaseTrack
    {
        Manual = 1,
        [EnumDescription("Pre-approved")]
        PreApproved,
        Automated
    }
}
