using System.Collections.Generic;
using ReMi.Common.Utils;

namespace ReMi.Plugin.ZenDesk.Models
{
    public class TicketsWrapper
    {
        public List<Ticket> Tickets { get; set; }

        public override string ToString()
        {
            return string.Format("[Tickets={0}]", Tickets.FormatElements());
        }
    }
}
