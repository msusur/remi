namespace ReMi.Plugin.ZenDesk.Models
{
    public class TicketWrapper
    {
        public Ticket Ticket { get; set; }

        public override string ToString()
        {
            return string.Format("[Ticket={0}]", Ticket);
        }
    }
}
