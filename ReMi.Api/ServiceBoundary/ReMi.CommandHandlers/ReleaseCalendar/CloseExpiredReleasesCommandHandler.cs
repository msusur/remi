using System;
using System.Linq;
using Common.Logging;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessLogic.ReleasePlan;
using ReMi.Commands.ReleaseCalendar;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseExecution;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;

namespace ReMi.CommandHandlers.ReleaseCalendar
{
    public class CloseExpiredReleasesCommandHandler : IHandleCommand<CloseExpiredReleasesCommand>
    {
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();

        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }
        public IReleaseWindowStateUpdater ReleaseWindowStateUpdater { get; set; }
        public Func<IReleaseApproverGateway> ApproversGatewayFactory { get; set; }
        public Func<ISignOffGateway> SignersGatewayFactory { get; set; }
        public IReleaseWindowHelper ReleaseWindowHelper { get; set; }

        public void Handle(CloseExpiredReleasesCommand command)
        {
            using (var gateway = ReleaseWindowGatewayFactory())
            {
                var expiredReleases = gateway.GetExpiredReleases().Where(ReleaseWindowHelper.IsMaintenance);

                foreach (var release in expiredReleases)
                {
                    using (var approverGateway = ApproversGatewayFactory())
                        if (!release.ApprovedOn.HasValue && approverGateway.GetApprovers(release.ExternalId).Any())
                        {
                            continue;
                        }

                    using (var signerGateway = SignersGatewayFactory())
                        if (!release.SignedOff.HasValue && signerGateway.GetSignOffs(release.ExternalId).Any())
                        {
                            continue;
                        }

                    Logger.DebugFormat("Closing the release due to expiration. ReleaseWindowId={0}, EndTime={1}", 
                        release.ExternalId, release.EndTime);

                    ReleaseWindowStateUpdater.CloseRelease(
                        release.ExternalId,
                        "Closing automatically due to expiration",
                        Enumerable.Empty<Account>(),
                        command.CommandContext.UserId);
                }
            }
        }
    }
}
