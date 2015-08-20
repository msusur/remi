using ReMi.Contracts.Enums;

namespace ReMi.Contracts.Plugins.Data.SourceControl
{
    public enum SourceControlRetrieveMode
    {
        [EnumDescription("Deployment jobs")]
        DeploymentJobs,
        [EnumDescription("Repository Identifier")]
        RepositoryIdentifier,
        None
    }
}
