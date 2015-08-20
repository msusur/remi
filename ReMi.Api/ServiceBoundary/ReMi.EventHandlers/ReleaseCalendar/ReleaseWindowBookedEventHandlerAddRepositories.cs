using Common.Logging;
using ReMi.BusinessLogic.ReleasePlan;
using ReMi.Commands.SourceControl;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Events.ReleaseCalendar;

namespace ReMi.EventHandlers.ReleaseCalendar
{
    public class ReleaseWindowBookedEventHandlerAddRepositories : IHandleEvent<ReleaseWindowBookedEvent>
    {
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();

        public ICommandDispatcher CommandDispatcher { get; set; }
        public IReleaseWindowHelper ReleaseWindowHelper { get; set; }

        public void Handle(ReleaseWindowBookedEvent evnt)
        {
            if (ReleaseWindowHelper.IsMaintenance(evnt.ReleaseWindow))
            {
                Logger.DebugFormat("Cancel adding repositories to maintenance window. Type={0}, ExternalId={1}",
                    evnt.ReleaseWindow.ReleaseType, evnt.ReleaseWindow.ExternalId);
                return;
            }

            CommandDispatcher.Send(new LoadReleaseRepositoriesCommand
            {
                CommandContext = evnt.Context.CreateChild<CommandContext>(),
                ReleaseWindowId = evnt.ReleaseWindow.ExternalId
            });
        }
    }
}
