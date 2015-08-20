using AutoMapper;
using ReMi.Contracts.Plugins.Data.HelpDesk;
using ReMi.Contracts.Plugins.Services.HelpDesk;
using ReMi.Plugin.ZenDesk.Models;
using System;
using System.Collections.Generic;

namespace ReMi.Plugin.ZenDesk
{
    public class ZenDeskService : IHelpDeskService
    {
        public IZenDeskRequest ZenDeskRequest { get; set; }
        public IMappingEngine Mapper { get; set; }

        public HelpDeskTicket CreateTicket(HelpDeskTicket ticket, IEnumerable<Guid> packageIds)
        {
            var result = ZenDeskRequest.CreateTicket(Mapper.Map<HelpDeskTicket, Ticket>(ticket));
            return Mapper.Map<Ticket, HelpDeskTicket>(result);
        }

        public HelpDeskTicket UpdateTicket(HelpDeskTicket ticket, IEnumerable<Guid> packageIds)
        {
            var result = ZenDeskRequest.UpdateTicket(Mapper.Map<HelpDeskTicket, Ticket>(ticket));
            return Mapper.Map<Ticket, HelpDeskTicket>(result);
        }

        public void DeleteTicket(string ticketRef, IEnumerable<Guid> packageIds)
        {
            ZenDeskRequest.DeleteTicket(ticketRef);
        }
    }
}
