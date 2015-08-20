using ReMi.BusinessEntities;
using ReMi.BusinessEntities.ReleasePlan;
using System;
using System.Collections.Generic;

namespace ReMi.DataAccess.BusinessEntityGateways.ReleasePlan
{
    public interface IReleaseContentGateway : IDisposable
    {
        IEnumerable<ReleaseContentTicket> GetTicketInformations(IEnumerable<Guid> ticketIds);
        IEnumerable<ReleaseContentTicket> GetTicketInformations(Guid releaseWindowId);
        IEnumerable<EnumEntry> GetTicketRisk();
        
        void AddOrUpdateTicketComment(ReleaseContentTicket ticket);
        void AddOrUpdateTicketRisk(ReleaseContentTicket ticket);
        void AddOrUpdateTickets(IEnumerable<ReleaseContentTicket> tickets, Guid lastEditor, Guid releaseWindowId);
        void UpdateTicketReleaseNotesRelation(ReleaseContentTicket ticket, Guid accountId);
        void CreateTicket(ReleaseContentTicket ticket, Guid account);
        
        void RemoveTicketsFromRelease(Guid releaseWindowId);
    }
}
