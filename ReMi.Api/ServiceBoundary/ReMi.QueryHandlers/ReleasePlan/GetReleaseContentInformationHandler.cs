using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ReMi.BusinessEntities.Products;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Contracts.Plugins.Services.ReleaseContent;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Queries.ReleasePlan;

namespace ReMi.QueryHandlers.ReleasePlan
{
    public class GetReleaseContentInformationHandler :
        IHandleQuery<GetReleaseContentInformationRequest, GetReleaseContentInformationResponse>
    {
        public Func<IProductGateway> ProductGatewayFactory { get; set; }
        public Func<IReleaseContentGateway> ReleaseContentGateway { get; set; }
        public IReleaseContent ReleaseContent { get; set; }
        public IMappingEngine MappingEngine { get; set; }

        public GetReleaseContentInformationResponse Handle(GetReleaseContentInformationRequest request)
        {
            var tickets = (GetExistingTickets(request.ReleaseWindowId) ?? Enumerable.Empty<ReleaseContentTicket>()).ToList();
            if (!tickets.IsNullOrEmpty())
            {
                return new GetReleaseContentInformationResponse
                {
                    Content = new List<ReleaseContentTicket>(tickets)
                };
            }

            IEnumerable<Product> products;
            using (var productGateway = ProductGatewayFactory())
            {
                products = productGateway.GetProducts(request.ReleaseWindowId).ToList();
            }

            var issues = ReleaseContent.GetTickets(products.Select(x => x.ExternalId)).ToList();
            var chooseTicketsByDefault = products.Any(o => o.ChooseTicketsByDefault);

            tickets = MappingEngine.Map<IEnumerable<Contracts.Plugins.Data.ReleaseContent.ReleaseContentTicket>, IEnumerable<ReleaseContentTicket>>(issues).ToList();
            tickets.ForEach(x => x.IncludeToReleaseNotes = chooseTicketsByDefault);

            MapExistingTicketInformation(tickets);

            return new GetReleaseContentInformationResponse
            {
                Content = new List<ReleaseContentTicket>(tickets)
            };
        }

        private IEnumerable<ReleaseContentTicket> GetExistingTickets(Guid releaseWindowId)
        {
            using (var gateway = ReleaseContentGateway())
            {
                return gateway.GetTicketInformations(releaseWindowId);
            }
        }

        private void MapExistingTicketInformation(IEnumerable<ReleaseContentTicket> tickets)
        {
            if (tickets.IsNullOrEmpty()) return;

            using (var gateway = ReleaseContentGateway())
            {
                var ticketInformations = gateway.GetTicketInformations(tickets.Select(x => x.TicketId).ToArray());
                if (ticketInformations.IsNullOrEmpty()) return;

                foreach (var ticket in tickets.Where(x => ticketInformations.Any(y => y.TicketId == x.TicketId)))
                {
                    var ticketInformation = ticketInformations.First(x => x.TicketId == ticket.TicketId);
                    ticket.Comment = ticketInformation.Comment;
                    ticket.Risk = ticketInformation.Risk;
                    ticket.IncludeToReleaseNotes = ticketInformation.IncludeToReleaseNotes;
                }
            }
        }
    }
}
