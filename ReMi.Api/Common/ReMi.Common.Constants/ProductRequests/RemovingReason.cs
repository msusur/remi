using ReMi.Contracts.Enums;

namespace ReMi.Common.Constants.ProductRequests
{
    public enum RemovingReason
    {
        [EnumDescription("Closed and live")]
        ClosedAndLive = 1,
        [EnumDescription("Not needed")]
        NotNeeded,
        [EnumDescription("Opened by mistake")]
        OpenedByMistake,
        Other
    }
}
