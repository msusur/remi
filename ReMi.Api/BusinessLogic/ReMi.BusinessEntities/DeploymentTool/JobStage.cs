using ReMi.Contracts.Enums;

namespace ReMi.BusinessEntities.DeploymentTool
{
    public enum JobStage
    {
        [EnumDescription("Before release")]
        BeforeRelease,
        [EnumDescription("Before offline")]
        BeforeOffline,
        [EnumDescription("During offline")]
        DuringOffline,
        [EnumDescription("After offline")]
        AfterOffline,
        [EnumDescription("After release")]
        AfterRelease
    }
}
