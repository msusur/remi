using AutoMapper;
using ReMi.BusinessEntities;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Common.Utils;
using ReMi.DataEntities.ReleaseCalendar;
using ReMi.DataEntities.ReleasePlan;
using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.Common.Utils.Repository;
using DataAccount = ReMi.DataEntities.Auth.Account;

namespace ReMi.DataAccess.BusinessEntityGateways.ReleasePlan
{
    public class ReleaseContentGateway : BaseGateway, IReleaseContentGateway
    {
        public IRepository<ReleaseContent> ReleaseContentRepository { get; set; }
        public IRepository<ReleaseWindow> ReleaseWindowRepository { get; set; }
        public IRepository<TicketRiskDescription> TicketRiskRepository { get; set; }
        public IRepository<DataAccount> AccountRepository { get; set; }

        public IMappingEngine Mapper { get; set; }

        public IEnumerable<ReleaseContentTicket> GetTicketInformations(IEnumerable<Guid> ticketIds)
        {
            var list = ticketIds.ToList();
            return ReleaseContentRepository
                .GetAllSatisfiedBy(ticket => list.Contains(ticket.TicketId))
                .Select(x => Mapper.Map<ReleaseContent, ReleaseContentTicket>(x))
                .ToList();
        }

        public IEnumerable<ReleaseContentTicket> GetTicketInformations(Guid releaseWindowId)
        {
            var tickets = ReleaseWindowRepository.GetSatisfiedBy(x => x.ExternalId == releaseWindowId).ReleaseContent;
            if (tickets.IsNullOrEmpty() || tickets.All(x => !x.IncludeToReleaseNotes)) return null;

            return tickets.Where(x => x.IncludeToReleaseNotes)
                .Select(x => Mapper.Map<ReleaseContent, ReleaseContentTicket>(x));
        }

        public IEnumerable<EnumEntry> GetTicketRisk()
        {
            return TicketRiskRepository.Entities.Select(x => new EnumEntry
            {
                Value = x.Name,
                Text = x.Description
            }).ToList();
        }

        public void AddOrUpdateTicketComment(ReleaseContentTicket ticket)
        {
            if (ticket == null)
                throw new ArgumentNullException("ticket");

            var account = AccountRepository.GetSatisfiedBy(x => x.ExternalId == ticket.LastChangedByAccount);

            if (ReleaseContentRepository.Entities.Any(x => x.TicketId == ticket.TicketId))
            {
                var entity = ReleaseContentRepository.Entities.First(x => x.TicketId == ticket.TicketId);
                entity.Comment = ticket.Comment;
                UpdateTicketInformationRepository(account.AccountId, entity);
            }
            else
            {
                CreateTicket(ticket, account.AccountId);
            }
        }

        public void AddOrUpdateTicketRisk(ReleaseContentTicket ticket)
        {
            if (ticket == null)
                throw new ArgumentNullException("ticket");

            var account = AccountRepository.GetSatisfiedBy(x => x.ExternalId == ticket.LastChangedByAccount);

            if (ReleaseContentRepository.Entities.Any(x => x.TicketId == ticket.TicketId))
            {
                var entity = ReleaseContentRepository.Entities.First(x => x.TicketId == ticket.TicketId);
                entity.TicketRisk = ticket.Risk;
                UpdateTicketInformationRepository(account.AccountId, entity);
            }
            else
            {
                CreateTicket(ticket, account.AccountId);
            }
        }

        public void AddOrUpdateTickets(IEnumerable<ReleaseContentTicket> tickets, Guid lastEditor, Guid releaseWindowId)
        {
            if (tickets.IsNullOrEmpty())
                throw new ArgumentNullException("tickets");

            var account = AccountRepository.GetSatisfiedBy(x => x.ExternalId == lastEditor);
            var releaseWindow = ReleaseWindowRepository.GetSatisfiedBy(x => x.ExternalId == releaseWindowId);

            foreach (var ticket in tickets)
            {
                ReleaseContent entity;
                if (ReleaseContentRepository.Entities.Any(x => x.TicketId == ticket.TicketId))
                {
                    entity = ReleaseContentRepository.Entities.First(x => x.TicketId == ticket.TicketId);
                    entity.Description = ticket.TicketDescription;
                    entity.Assignee = ticket.Assignee;
                    entity.ReleaseWindow = null;
                    entity.ReleaseWindowsId = releaseWindow.ReleaseWindowId;
                    entity.IncludeToReleaseNotes = ticket.IncludeToReleaseNotes;
                    UpdateTicketInformationRepository(account.AccountId, entity);
                }
                else
                {
                    CreateTicket(ticket, account.AccountId, releaseWindow.ReleaseWindowId);
                }
            }
        }

        public void CreateTicket(ReleaseContentTicket ticket, Guid lastEditor)
        {
            var account = AccountRepository.GetSatisfiedBy(x => x.ExternalId == lastEditor);

            CreateTicket(ticket, account.AccountId);
        }

        public void RemoveTicketsFromRelease(Guid releaseWindowId)
        {
            var tickets =
                ReleaseContentRepository.GetAllSatisfiedBy(x => x.ReleaseWindow.ExternalId == releaseWindowId);

            if (tickets != null)
            {
                Logger.InfoFormat("Removing tickets from release {0}...", releaseWindowId);

                foreach (var ticket in tickets)
                {
                    ticket.ReleaseWindow = null;
                    ticket.ReleaseWindowsId = null;

                    ReleaseContentRepository.Update(ticket);

                    Logger.InfoFormat("Ticket removed from release. ReleaseWindowId={0}, TicketInformationId={1}",
                        releaseWindowId, ticket.ReleaseContentId);
                }
            }
        }

        public void UpdateTicketReleaseNotesRelation(ReleaseContentTicket ticket, Guid accountId)
        {
            var account = AccountRepository.GetSatisfiedBy(x => x.ExternalId == accountId);

            var entity = ReleaseContentRepository.Entities.First(x => x.TicketId == ticket.TicketId);
            entity.IncludeToReleaseNotes = ticket.IncludeToReleaseNotes;
            UpdateTicketInformationRepository(account.AccountId, entity);
        }

        private ReleaseContent CreateTicket(ReleaseContentTicket ticket, int accountId, int? releaseWindowId = null)
        {
            var entity = Mapper.Map<ReleaseContentTicket, ReleaseContent>(ticket);
            entity.LastChangedByAccount = null;
            entity.LastChangedByAccountId = accountId;
            if (releaseWindowId.HasValue)
            {
                entity.ReleaseWindow = null;
                entity.ReleaseWindowsId = releaseWindowId.Value;
            }
            ReleaseContentRepository.Insert(entity);

            return entity;
        }

        private void UpdateTicketInformationRepository(int accountId, ReleaseContent entity)
        {
            entity.LastChangedByAccountId = accountId;
            ReleaseContentRepository.Update(entity);
        }
    }
}
