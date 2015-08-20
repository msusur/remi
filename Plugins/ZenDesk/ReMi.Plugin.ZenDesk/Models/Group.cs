using System;

namespace ReMi.Plugin.ZenDesk.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Deleted { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
