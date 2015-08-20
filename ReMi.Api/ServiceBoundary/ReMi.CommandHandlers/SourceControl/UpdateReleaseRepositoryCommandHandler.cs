using ReMi.Commands.SourceControl;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using System;

namespace ReMi.CommandHandlers.SourceControl
{
    public class UpdateReleaseRepositoryCommandHandler : IHandleCommand<UpdateReleaseRepositoryCommand>
    {
        public Func<IReleaseRepositoryGateway> ReleaseRepositoryGatewayFactory { get; set; }
 
        public void Handle(UpdateReleaseRepositoryCommand command)
        {
            using (var gateway = ReleaseRepositoryGatewayFactory())
            {
                var repository = gateway.GetReleaseRepository(command.ReleaseWindowId, command.Repository.ExternalId);
                if (repository == null)
                    gateway.AddRepositoryToRelease(command.Repository, command.ReleaseWindowId);
                else
                    gateway.UpdateRepository(command.Repository, command.ReleaseWindowId);
            }
        }
    }
}
