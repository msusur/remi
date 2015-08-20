using ReMi.BusinessEntities.HelpDesk;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.ReleasePlan
{
    public class HelpDeskAttachmentCreatedEvent : IEvent
    {
        public HelpDeskTicketAttachment HelpDeskAttachment { get; set; }

        public object Data
        {
            get { return HelpDeskAttachment; }
            set { HelpDeskAttachment = (HelpDeskTicketAttachment)value; }
        }

        public EventContext Context { get; set; }
    }
}
