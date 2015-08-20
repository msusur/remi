using System;
using System.Collections.Generic;

namespace ReMi.Plugin.Gerrit.GerritApi.Model
{
    public class GitLogEntity
    {
        public string Hash { get; set; }
        public string Author { get; set; }
        public DateTime Date { get; set; }
        public string Reference { get; set; }
        public string Subject { get; set; }
        public IEnumerable<string> JiraTickets { get; set; }
        public CommitType CommitType { get; set; }
        public string ChangeId { get; set; }
    }
}
