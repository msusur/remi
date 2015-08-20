using System;

namespace ReMi.Plugin.Jira.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Body { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public User Author { get; set; }
    }
}
