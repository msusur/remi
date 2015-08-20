using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ReMi.BusinessEntities.Api;
using ReMi.BusinessLogic.Api;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.Api;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.Queries.ContinuousDelivery;

namespace ReMi.QueryHandlers.ContinuousDelivery
{
    public class GetApiDescriptionHandler : IHandleQuery<GetApiDescriptionRequest, GetApiDescriptionResponse>
    {
        public Func<IApiDescriptionGateway> ApiDescriptionGatewayFactory { get; set; }
        public Func<ICommandPermissionsGateway> CommandPermissionsGatewayFactory { get; set; }
        public Func<IQueryPermissionsGateway> QueryPermissionsGatewayFactory { get; set; }
        public IApiDescriptionBuilder ApiBuilder { get; set; }
        public IMappingEngine MappingEngine { get; set; }

        public GetApiDescriptionResponse Handle(GetApiDescriptionRequest request)
        {
            var apiDescriptions = ApiBuilder.GetApiDescriptions().ToList();

            List<ApiDescription> dbApiDescriptions;
            using (var gateway = ApiDescriptionGatewayFactory())
            {
                dbApiDescriptions = gateway.GetApiDescriptions();
            }
            List<Command> dbCommands;
            using (var gateway = CommandPermissionsGatewayFactory())
            {
                dbCommands = gateway.GetCommands().ToList();
            }
            List<Query> dbQueries;
            using (var gateway = QueryPermissionsGatewayFactory())
            {
                dbQueries = gateway.GetQueries().ToList();
            }

            var result = new List<ApiDescriptionFull>();
            foreach (var apiDescription in apiDescriptions)
            {
                var fullDesc = MappingEngine.Map<ApiDescription, ApiDescriptionFull>(apiDescription);

                SetDescription(fullDesc, dbApiDescriptions);

                if (fullDesc.Method.Equals("post", StringComparison.InvariantCultureIgnoreCase))
                {
                    SetCommandInfo(fullDesc, dbCommands);
                }
                else
                {
                    SetQueryInfo(fullDesc, dbQueries);
                }

                result.Add(fullDesc);
            }

            return new GetApiDescriptionResponse
            {
                ApiDescriptions = result
            };
        }

        private void SetDescription(ApiDescriptionFull fullDescription, IList<ApiDescription> apiDescriptions)
        {
            var found = apiDescriptions.FirstOrDefault(o => o.Url == fullDescription.Url);
            if (found != null)
                fullDescription.Description = found.Description;
        }

        private void SetCommandInfo(ApiDescriptionFull fullDescription, IEnumerable<Command> dbCommands)
        {
            var command = dbCommands.FirstOrDefault(o => o.Name == fullDescription.Name);
            if (command != null)
            {
                fullDescription.DescriptionShort = command.Description;
                fullDescription.Group = command.Group;

                if (command.Roles != null && command.Roles.Any())
                {
                    fullDescription.Roles = string.Join(", ", command.Roles.Select(o => o.Description).ToArray());
                }
            }
        }

        private void SetQueryInfo(ApiDescriptionFull fullDescription, IEnumerable<Query> dbQueries)
        {
            var query = dbQueries.FirstOrDefault(o => o.Name.StartsWith(fullDescription.Name));
            if (query != null)
            {
                fullDescription.DescriptionShort = query.Description;
                fullDescription.Group = query.Group;

                if (query.Roles != null && query.Roles.Any())
                {
                    fullDescription.Roles = string.Join(", ", query.Roles.Select(o => o.Description).ToArray());
                }
            }
        }
    }
}
