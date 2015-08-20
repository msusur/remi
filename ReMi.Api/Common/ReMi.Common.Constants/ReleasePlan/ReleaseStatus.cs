using ReMi.Contracts.Enums;

namespace ReMi.Common.Constants.ReleasePlan
{
    public enum ReleaseStatus
    {
        [EnumDescription("Opened")]
        Opened,
        [EnumDescription("Approved")]
        Approved,
        [EnumDescription("Signed off")]
        SignedOff,
        [EnumDescription("Closed")]
        Closed
    }
}
