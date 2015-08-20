using System;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using ReMi.Commands.SourceControl;
using ReMi.Common.Constants;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Contracts.Plugins.Data.Exceptions;
using ReMi.Contracts.Plugins.Data.SourceControl;
using ReMi.DataAccess.BusinessEntityGateways.SourceControl;
using ReMi.Events;
using ReMi.Queries.ReleasePlan;

namespace ReMi.CommandHandlers.SourceControl
{
    public class StoreSourceControlChangesCommandHandler : IHandleCommand<StoreSourceControlChangesCommand>
    {
        private readonly ILog _logger = LogManager.GetCurrentClassLogger();

        public Func<ISourceControlChangeGateway> SourceControlChangeGatewayFactory { get; set; }
        public IHandleQuery<GetReleaseChangesRequest, GetReleaseChangesResponse> ChangesQuery { get; set; }
        public IPublishEvent PublishEvent { get; set; }

        public void Handle(StoreSourceControlChangesCommand command)
        {
            IEnumerable<SourceControlChange> changes = null;

            try
            {
                changes = ChangesQuery.Handle(new GetReleaseChangesRequest
                {
                    ReleaseWindowId = command.ReleaseWindowId,
                    IsBackground = true
                }).Changes.ToList();
            }
            catch (FailedToRetrieveSourceControlChangesException gx)
            {
                _logger.Error(gx);

                PublishEvent.Publish(new NotificationOccuredForUserEvent
                {
                    Type = NotificationOccuredEventType.Warning,
                    Code = "FailedToRetrieveSourceControlChanges",
                    Message = "Unable to attach Source Control changes to release window. Please request administrator for help",

                    Context = command.CommandContext.CreateChild<EventContext>()
                });
                return;
            }

            if (changes.IsNullOrEmpty()) return;

            using (var gateway = SourceControlChangeGatewayFactory())
            {
                gateway.StoreChanges(changes, command.ReleaseWindowId);
            }
        }
    }
}
