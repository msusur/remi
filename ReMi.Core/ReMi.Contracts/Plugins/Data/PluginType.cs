using System;
using ReMi.Contracts.Enums;

namespace ReMi.Contracts.Plugins.Data
{
    [Flags]
    public enum PluginType
    {
        [EnumDescription("Quality Assurance Statistics Service")]
        QaStats = 1,
        [EnumDescription("Release Content Service")]
        ReleaseContent = 2,
        [EnumDescription("Source Control Service")]
        SourceControl = 4,
        [EnumDescription("Authentication Service", Annotation = "global")]
        Authentication = 8,
        [EnumDescription("Email Servive", Annotation = "global")]
        Email = 16,
        [EnumDescription("Help Desk Service")]
        HelpDesk = 32,
        [EnumDescription("Deployment Tool")]
        DeploymentTool = 64
    }
}
