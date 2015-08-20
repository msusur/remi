using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.Commands.ReleasePlan;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Plugins.Services.HelpDesk;
using ReMi.DataAccess.BusinessEntityGateways.Products;

namespace ReMi.CommandHandlers.ReleasePlan
{
    public class DeleteHelpDeskTaskCommandHandler : IHandleCommand<DeleteHelpDeskTaskCommand>
    {
        public IHelpDeskService HelpDeskService { get; set; }
        public Func<IProductGateway> ProductGatewayFactory { get; set; }

        public void Handle(DeleteHelpDeskTaskCommand command)
        {
            IEnumerable<Guid> packagesIds;
            using (var gateway = ProductGatewayFactory())
            {
                packagesIds = gateway.GetProducts(command.ReleaseWindowId)
                    .Select(x => x.ExternalId)
                    .ToArray();
            }
            HelpDeskService.DeleteTicket(command.HelpDeskTicketRef, packagesIds);
        }
    }
}
