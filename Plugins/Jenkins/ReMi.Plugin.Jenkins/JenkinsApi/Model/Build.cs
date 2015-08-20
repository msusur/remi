
using System.Collections.Generic;

namespace ReMi.Plugin.Jenkins.JenkinsApi.Model
{
    public class Build
    {
        public int Number { get; set; }
        public string Url { get; set; }
        public IEnumerable<SubBuild> SubBuilds { get; set; } 
    }
}
