using System;

namespace ReMi.Plugin.Jira.Models
{
    public class Version
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Archived { get; set; }
        public bool Released { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ReleaseDate { get; set; }
    }
}
