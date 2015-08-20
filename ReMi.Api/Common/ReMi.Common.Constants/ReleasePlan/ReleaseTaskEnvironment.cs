using ReMi.Contracts.Enums;

namespace ReMi.Common.Constants.ReleasePlan
{
    public enum ReleaseTaskEnvironment
    {
        [EnumDescription("Local")]
        Local = 1,

        [EnumDescription("WIP")]
        Wip,

        [EnumDescription("Staging")]
        Staging,

        [EnumDescription("PreProd")]
        Preprod
    }
}
