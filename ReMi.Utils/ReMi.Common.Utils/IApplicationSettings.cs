namespace ReMi.Common.Utils
{
    public interface IApplicationSettings
    {
        int DefaultReleaseWindowDurationTime { get; }
        int SessionDuration { get; }
        bool LogJsonFormatted { get; }
        bool LogQueryResponses { get; }
        string FrontEndUrl { get; }
    }
}
