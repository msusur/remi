using ReMi.Contracts.Enums;

namespace ReMi.Common.Constants.Subscriptions
{
    public enum NotificationType
    {
        [EnumDescription("Api change")]
        ApiChange,
        [EnumDescription("Release booked")]
        ReleaseWindowsSchedule,
        [EnumDescription("Release approved")]
        Approvement,
        [EnumDescription("Release signed off")]
        Signing,
        [EnumDescription("Release closed")]
        Closing,
        [EnumDescription("Release task updated")]
        ReleaseTasks,
        [EnumDescription("Site maintenance mode start")]
        SiteDown,
        [EnumDescription("Site maintenance mode end")]
        SiteUp
    }
}
