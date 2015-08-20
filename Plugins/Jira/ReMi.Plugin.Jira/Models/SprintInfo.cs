using System.Collections.Generic;

namespace ReMi.Plugin.Jira.Models
{
    public class SprintInfo
    {
        public IEnumerable<Issue> Issues { get; set; }
        public bool IsReleased { get; set; }
        public string Sprint { get; set; }
    }
}
