using System;

namespace ReMi.Plugin.Jira.Models
{
    public class IssueFields
    {
        public string Summary { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public IssueType IssueType { get; set; }
        public IssueStatus Status { get; set; }
        public string[] Labels { get; set; }
        
        public User Reporter { get; set; }
        public User Assignee { get; set; }

        public Project Project { get; set; }

        public override string ToString()
        {
            return string.Format("[Summary={0}, Description={1}, Created={2}, IssueType={3}, Status={4}, Project={5}]",
                Summary, Description, Created, IssueType, Status, Project);
        }
    }
}
