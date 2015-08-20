
using ReMi.Contracts.Enums;

namespace ReMi.Contracts.Cqrs.Commands
{
    public enum QueryGroup
    {
        [EnumDescription("Access Control")]
        AccessControl,
        [EnumDescription("Release Plan")]
        ReleasePlan,
        [EnumDescription("Release Calendar")]
        ReleaseCalendar,
        [EnumDescription("Acknowledge Release")]
        AcknowledgeRelease,
        [EnumDescription("Api")]
        Api,
        [EnumDescription("Configuration")]
        Configuration,
        [EnumDescription("DeploymentTool")]
        DeploymentTool,
        [EnumDescription("Release Execution")]
        ReleaseExecution,
        [EnumDescription("Metrics")]
        Metrics,
        [EnumDescription("Continuous Delivery")]
        ContinuousDelivery,
        [EnumDescription("Business Rules")]
        BusinessRules,
        [EnumDescription("Product Requests")]
        ProductRequests,
        Subscriptions,
        Common,
        Plugins
    }
}
