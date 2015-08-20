using AutoMapper;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Commands.ReleasePlan;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Plugins.Data.HelpDesk;
using ReMi.Contracts.Plugins.Services.HelpDesk;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.DataAccess.BusinessEntityGateways.Products;

namespace ReMi.CommandHandlers.ReleasePlan
{
    public class UpdateHelpDeskTaskCommandHandler : IHandleCommand<UpdateHelpDeskTaskCommand>
    {
        public IPublishEvent EventPublisher { get; set; }
        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }
        public IHelpDeskService HelpDeskService { get; set; }
        public IMappingEngine MappingEngine { get; set; }
        public Func<IProductGateway> ProductGatewayFactory { get; set; }

        public void Handle(UpdateHelpDeskTaskCommand command)
        {
            var ticket = MappingEngine.Map<ReleaseTask, HelpDeskTicket>(command.ReleaseTask);
            using (var gateway = ReleaseWindowGatewayFactory())
            {
                var releaseWindow = gateway.GetByExternalId(command.ReleaseTask.ReleaseWindowId);
                ticket.Priority = releaseWindow.ReleaseType == ReleaseType.Scheduled ? "normal" : "high";
            }
            IEnumerable<Guid> packagesIds;
            using (var gateway = ProductGatewayFactory())
            {
                packagesIds = gateway.GetProducts(command.ReleaseTask.ReleaseWindowId)
                    .Select(x => x.ExternalId)
                    .ToArray();
            }
            HelpDeskService.UpdateTicket(ticket, packagesIds);
        }
    }
}
