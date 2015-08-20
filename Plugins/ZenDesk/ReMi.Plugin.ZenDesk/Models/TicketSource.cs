namespace ReMi.Plugin.ZenDesk.Models
{
    public class TicketSource
    {
        public TicketSourceSpec From { get; set; }
        public TicketSourceSpec To { get; set; }
        public string Rel { get; set; }
    }
}
