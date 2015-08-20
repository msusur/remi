using System;
using ReMi.Commands.ReleasePlan;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;

namespace ReMi.CommandHandlers.ReleasePlan
{
    public class UpdateReleaseJobCommandHandler : IHandleCommand<UpdateReleaseJobCommand>
    {
        public Func<IReleaseJobGateway> ReleaseJobFuncGatewayFactory { get; set; }

        public void Handle(UpdateReleaseJobCommand command)
        {
            using (var gateway = ReleaseJobFuncGatewayFactory())
            {
                var existingReleaseJob = gateway.GetReleaseJob(command.ReleaseWindowId, command.ReleaseJob.JobId);
                if (existingReleaseJob != null && !command.ReleaseJob.IsIncluded)
                {
                    gateway.RemoveJobFromRelease(command.ReleaseJob.JobId, command.ReleaseWindowId);
                }
                else if (existingReleaseJob == null && command.ReleaseJob.IsIncluded)
                {
                    command.ReleaseJob.ExternalId = Guid.NewGuid();
                    gateway.AddJobToRelease(command.ReleaseJob, command.ReleaseWindowId);
                }
            }
        }
    }
}
