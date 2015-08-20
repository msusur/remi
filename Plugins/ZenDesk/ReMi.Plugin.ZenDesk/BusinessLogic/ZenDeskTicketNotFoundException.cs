using System;

namespace ReMi.Plugin.ZenDesk
{
    public class ZenDeskTicketNotFoundException : ApplicationException
    {
        public ZenDeskTicketNotFoundException(string ticketRef)
            : base(FormatMessage(ticketRef))
        {

        }
        private static string FormatMessage(string ticketRef)
        {
            return string.Format("ZenDesk ticket cound't be found. Ticket Ref: {0}", ticketRef);
        }
    }
}
