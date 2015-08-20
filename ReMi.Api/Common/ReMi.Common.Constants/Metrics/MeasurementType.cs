using ReMi.Contracts.Enums;

namespace ReMi.Common.Constants.Metrics
{
    public enum MeasurementType
    {
        [EnumDescription("Pre-down time")]
        PreDownTime,
        [EnumDescription("Post-down time")]
        PostDownTime,
        [EnumDescription("Down time")]
        DownTime,
        [EnumDescription("Deploy time")]
        DeployTime,
        [EnumDescription("Overall time")]
        OverallTime,
        [EnumDescription("Run time")]
        RunTime
    }
}
