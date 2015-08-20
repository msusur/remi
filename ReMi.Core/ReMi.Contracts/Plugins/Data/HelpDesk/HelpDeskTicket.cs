
namespace ReMi.Contracts.Plugins.Data.HelpDesk
{
    public class HelpDeskTicket
    {
        public string Id { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string Priority { get; set; }
        public string Comment { get; set; }
    }
}
