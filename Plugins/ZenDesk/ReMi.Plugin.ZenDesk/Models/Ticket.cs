using System;
using System.Linq;

namespace ReMi.Plugin.ZenDesk.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public Statuses Status { get; set; }
        public Types Type { get; set; }
        public Priorities Priority { get; set; }
        public int? Assignee_Id { get; set; }
        public int? Requester_Id { get; set; }
        public int? Organization_Id { get; set; }
        public int? Group_Id { get; set; }
        public DateTime Created_At { get; set; }
        public DateTime Updated_At { get; set; }
        public DateTime? Due_At { get; set; }
        public Guid? CreatedByExternalId { get; set; }

        /// <summary>
        /// API URL address
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// URL address to ticket web page
        /// </summary>
        public string TicketUrl
        {
            get
            {
                return string.IsNullOrEmpty(Url) ? null : Url.Replace("api/v2/", string.Empty).Replace(".json", string.Empty);
            }
        }


        private string[] _tags;

        public string[] Tags
        {
            get
            {
                if (_tags == null)
                    return new[] { Type.ToString() };

                if (_tags.Contains(Type.ToString()))
                    return _tags;

                return _tags.Concat(new[] { Type.ToString() }).ToArray();
            }
            set { _tags = value; }
        }


        public TicketComment Comment { get; set; }

        public TicketVia Via { get; set; }

        public override string ToString()
        {
            return string.Format("[Id={0}, Subject={1}, Description={2}, Status={3}, Priority={4}, Type={5}, TicketUrl={6}, Created_At={7}]",
                Id, Subject, Description, Status, Priority, Type, TicketUrl, Created_At);
        }
    }
}
