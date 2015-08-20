using System.Collections.Generic;

namespace ReMi.Plugin.Jenkins.JenkinsApi.Model
{
    public class JobInfo
    {
        public string DisplayName { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }

        public IEnumerable<Build> Builds { get; set; }
    }
}
