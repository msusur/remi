using System.Collections.Generic;

namespace ReMi.Plugin.Jira.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<IssueType> IssueTypes { get; set; }
        public List<Version> Versions { get; set; }

        public override string ToString()
        {
            return string.Format("[Id={0}, Name={1}]", Id, Name);
        }
    }
}
