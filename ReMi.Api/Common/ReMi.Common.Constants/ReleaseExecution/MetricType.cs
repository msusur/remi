using ReMi.Contracts.Enums;

namespace ReMi.Common.Constants.ReleaseExecution
{
    public enum MetricType
    {
        [EnumDescription("Site down"), EnumOrder(3)]
        SiteDown = 1,
        [EnumDescription("Start deploy"), EnumOrder(5)]
        StartDeploy,
        [EnumDescription("Finish deploy"), EnumOrder(6)]
        FinishDeploy,
        [EnumDescription("Site up"), EnumOrder(8)]
        SiteUp,
        [EnumDescription("Start run"), EnumOrder(4)]
        StartRun,
        [EnumDescription("Finish run"), EnumOrder(7)]
        FinishRun,
        [EnumDescription("Complete"), EnumOrder(9)]
        Complete,
        [EnumDescription("Start time", Annotation = "IsBackground"), EnumOrder(2)]
        StartTime,
        [EnumDescription("Approve", Annotation = "IsBackground"), EnumOrder(1)]
        Approve,
        [EnumDescription("Sign off", Annotation = "IsBackground"), EnumOrder(10)]
        SignOff,
        [EnumDescription("Close", Annotation = "IsBackground"), EnumOrder(11)]
        Close,
        [EnumDescription("End time", Annotation = "IsBackground"), EnumOrder(12)]
        EndTime,
    }
}
