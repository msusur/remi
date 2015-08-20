
using ReMi.Contracts.Enums;

namespace ReMi.Plugin.Jenkins
{
    public enum TimeZone
    {
        [EnumDescription("UTC (GMT)")]
        Utc,
        [EnumDescription("BST (British Summer Time)")]
        Bst
    }
}
