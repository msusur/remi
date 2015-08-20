using AutoMapper;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Commands.ReleasePlan;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Events.ReleasePlan;
using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.BusinessEntities.HelpDesk;
using ReMi.Contracts.Plugins.Data.HelpDesk;
using ReMi.Contracts.Plugins.Services.HelpDesk;
using ReMi.DataAccess.BusinessEntityGateways.Products;

namespace ReMi.CommandHandlers.ReleasePlan
{
    public class CreateHelpDeskTaskCommandHandler : IHandleCommand<CreateHelpDeskTaskCommand>
    {
        public IPublishEvent EventPublisher { get; set; }
        public Func<IReleaseTaskGateway> ReleaseTaskGatewayFactory { get; set; }
        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }
        public Func<IProductGateway> ProductGatewayFactory { get; set; }
        public IHelpDeskService HelpDeskService { get; set; }
        public IMappingEngine MappingEngine { get; set; }
        public IApplicationSettings ApplicationSettings { get; set; }

        public void Handle(CreateHelpDeskTaskCommand command)
        {
            using (var gateway = ReleaseTaskGatewayFactory())
            {
                var dbTask = gateway.GetReleaseTask(command.ReleaseTask.ExternalId);
                if (!string.IsNullOrEmpty(dbTask.HelpDeskTicketReference))
                    return;
            }

            #region TODO
            //TODO: Should be removed. Argument 'task' should has both display name and value fields.

            var tasktype = command.ReleaseTask.Type;
            using (var gateway = ReleaseTaskGatewayFactory())
            {
                var taskTypeDescription = gateway.GetReleaseTaskTypes().FirstOrDefault(o => o.Value == tasktype);
                if (taskTypeDescription != null)
                    tasktype = taskTypeDescription.Text;
            }
            #endregion

            var ticket = MappingEngine.Map<ReleaseTask, HelpDeskTicket>(command.ReleaseTask);
            IEnumerable<Guid> packagesIds;
            using (var gateway = ProductGatewayFactory())
            {
                packagesIds = gateway.GetProducts(command.ReleaseTask.ReleaseWindowId)
                    .Select(x => x.ExternalId)
                    .ToArray();
            }
            using (var gateway = ReleaseWindowGatewayFactory())
            {
                var releaseWindow = gateway.GetByExternalId(command.ReleaseTask.ReleaseWindowId);

                ticket.Subject = string.Format("{0} {1} {2} [{3}]",
                    tasktype,
                    releaseWindow.Products.FormatElements(string.Empty, string.Empty),
                    releaseWindow.Sprint,
                    releaseWindow.StartTime.Date.ToShortDateString());

                ticket.Priority = releaseWindow.ReleaseType == ReleaseType.Scheduled ? "normal" : "high";

                var url = string.Format("{0}release?{1}={2}", ApplicationSettings.FrontEndUrl,
                    "releaseWindowId", releaseWindow.ExternalId);
                ticket.Comment += Environment.NewLine + url;
            }

            ticket = HelpDeskService.CreateTicket(ticket, packagesIds);
            if (ticket == null) return;

            using (var gateway = ReleaseTaskGatewayFactory())
            {
                gateway.AssignHelpDeskTicket(command.ReleaseTask, ticket.Id, ticket.Url);
            }

            var helpDeskTask = MappingEngine.Map<HelpDeskTicket, HelpDeskTask>(ticket);
            helpDeskTask.ReleaseTaskId = command.ReleaseTask.ExternalId;
            helpDeskTask.ReleaseWindowId = command.ReleaseTask.ReleaseWindowId;

            EventPublisher.Publish(new HelpDeskTaskCreatedEvent { HelpDeskTask = helpDeskTask });
        }
    }
}
