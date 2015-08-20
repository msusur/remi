using System.Collections.Generic;

namespace ReMi.Plugin.ZenDesk.Models
{
    public class TicketComment
    {
        public int Id { get; set; }
        public string Body { get; set; }
        public bool Public { get; set; }

        public TicketComment()
        {
            Public = true;
        }

        public IEnumerable<Attachment> Attachments { get; set; }
        public IEnumerable<string> Uploads { get; set; }

        public override string ToString()
        {
            return string.Format("[Id={0}, Public={1}, Body={2}]", Id, Public, Body);
        }
    }
}
