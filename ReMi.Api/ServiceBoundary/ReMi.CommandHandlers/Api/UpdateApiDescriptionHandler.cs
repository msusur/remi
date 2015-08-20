using System;
using ReMi.Commands;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.Api;

namespace ReMi.CommandHandlers.Api
{
    public class UpdateApiDescriptionHandler : IHandleCommand<UpdateApiDescriptionCommand>
    {
        public Func<IApiDescriptionGateway> ApiDescriptionGatewayFactory { get; set; }

        public void Handle(UpdateApiDescriptionCommand command)
        {
            using (var gateway = ApiDescriptionGatewayFactory())
            {
                gateway.UpdateApiDescription(command.ApiDescription);
            }
        }
    }
}
