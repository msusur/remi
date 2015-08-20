using System;

namespace ReMi.Plugin.ZenDesk
{
    public class ZenDeskFailedToUpdateTicketStatusException : ApplicationException
    {
        public ZenDeskFailedToUpdateTicketStatusException(int ticketRef)
            : base(FormatMessage(ticketRef))
        {

        }
        private static string FormatMessage(int ticketRef)
        {
            return string.Format("ZenDesk ticket cound't be resolved: {0}", ticketRef);
        }
    }
}
