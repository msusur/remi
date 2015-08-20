using System;
using System.Collections.Generic;
using ReMi.Contracts.Plugins.Data.HelpDesk;

namespace ReMi.Contracts.Plugins.Services.HelpDesk
{
    public interface IHelpDeskService : IPluginService
    {
        HelpDeskTicket CreateTicket(HelpDeskTicket ticket, IEnumerable<Guid> packageIds);
        HelpDeskTicket UpdateTicket(HelpDeskTicket ticket, IEnumerable<Guid> packageIds);
        void DeleteTicket(string ticketRef, IEnumerable<Guid> packageIds);
    }
}
