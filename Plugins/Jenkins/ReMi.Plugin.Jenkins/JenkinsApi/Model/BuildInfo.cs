using System;

namespace ReMi.Plugin.Jenkins.JenkinsApi.Model
{
    public class BuildInfo
    {

        public long Duration { get; set; }
        public long Timestamp { get; set; }
        public int Number { get; set; }
        public string Url { get; set; }
        public string Result { get; set; }
        public string Id { get; set; }

        public DateTime GetStartTime(TimeZone timeZone)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, timeZone == TimeZone.Utc ? DateTimeKind.Utc : DateTimeKind.Local).AddMilliseconds(Timestamp);
        }

        public DateTime GetEndTime(TimeZone timeZone)
        {
            return GetStartTime(timeZone).AddMilliseconds(Duration);
        }
    }
}
