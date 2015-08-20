using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReMi.Plugin.Jenkins.JenkinsApi.Model
{
    public class SubBuild
    {
        public int BuildNumber { get; set; }
        public string JobName { get; set; }
        public string PhaseName { get; set; }
        public string Result { get; set; }
        public string Url { get; set; }
    }
}
