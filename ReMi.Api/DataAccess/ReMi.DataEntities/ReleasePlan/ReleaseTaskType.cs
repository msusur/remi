using ReMi.Contracts.Enums;

namespace ReMi.DataEntities.ReleasePlan
{
    public enum ReleaseTaskType
    {
        [EnumDescription("Pre-release")]
        PreRelease = 1,
        [EnumDescription("Post-release")]
        PostRelease,
        [EnumDescription("Deployment Task")]
        DeploymentTask
    }
}
