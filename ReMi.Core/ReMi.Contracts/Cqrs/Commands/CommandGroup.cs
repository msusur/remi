
using ReMi.Contracts.Enums;

namespace ReMi.Contracts.Cqrs.Commands
{
    public enum CommandGroup
    {
        [EnumDescription("Access Control")]
        AccessControl,
        [EnumDescription("Release Plan")]
        ReleasePlan,
        [EnumDescription("Release Tasks")]
        ReleaseTask,
        [EnumDescription("Release Calendar")]
        ReleaseCalendar,
        [EnumDescription("Acknowledge Release")]
        AcknowledgeRelease,
        [EnumDescription("Configuration")]
        Configuration,
        [EnumDescription("Source Control")]
        SourceControl,
        [EnumDescription("Deployment Tool")]
        DeploymentTool,
        [EnumDescription("Release Execution")]
        ReleaseExecution,
        [EnumDescription("Metrics")]
        Metrics,
        [EnumDescription("Continuous Delivery")]
        ContinuousDelivery,
        [EnumDescription("Product Request")]
        ProductRequest,
        [EnumDescription("Business Rules")]
        BusinessRules,
        Subscriptions,
        Api,
        Plugins
    }
}
