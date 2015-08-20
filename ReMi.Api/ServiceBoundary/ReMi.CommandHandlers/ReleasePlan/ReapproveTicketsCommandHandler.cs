using System;
using System.Linq;
using ReMi.Commands.ReleasePlan;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Queries.ReleasePlan;

namespace ReMi.CommandHandlers.ReleasePlan
{
    public class ReapproveTicketsCommandHandler : IHandleCommand<ReapproveTicketsCommand>
    {
        public Func<IReleaseContentGateway> ReleaseContentGatewayFactory { get; set; }
        public IHandleQuery<GetReleaseContentInformationRequest, GetReleaseContentInformationResponse> GetReleaseContentInformationQuery { get; set; }

        public void Handle(ReapproveTicketsCommand command)
        {
            using (var gateway = ReleaseContentGatewayFactory())
            {
                gateway.RemoveTicketsFromRelease(command.ReleaseWindowId);

                var content = GetReleaseContentInformationQuery.Handle(
                    new GetReleaseContentInformationRequest { ReleaseWindowId = command.ReleaseWindowId });
                content.Content.ToList().ForEach(x => x.IncludeToReleaseNotes = true);

                gateway.AddOrUpdateTickets(content.Content, command.CommandContext.UserId, command.ReleaseWindowId);
            }
        }
    }
}
