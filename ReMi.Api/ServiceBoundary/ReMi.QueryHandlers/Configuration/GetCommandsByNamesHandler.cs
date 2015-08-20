using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataAccess.Exceptions;
using ReMi.Queries.Configuration;

namespace ReMi.QueryHandlers.Configuration
{
    public class GetCommandsByNamesHandler : IHandleQuery<GetCommandsByNamesRequest, GetCommandsByNamesResponse>
    {
        public Func<ICommandPermissionsGateway> CommandPermissionsGatewayFactory { get; set; }

        public GetCommandsByNamesResponse Handle(GetCommandsByNamesRequest request)
        {
            var names = ParseCommandNames(request.Names).Where(o => !string.IsNullOrWhiteSpace(o)).ToArray();
            if (names.Length == 0)
                throw new CommandListNotFoundException(request.Names);

            using (var gateway = CommandPermissionsGatewayFactory())
            {
                var commands = gateway.GetCommands()
                    .Where(o => names.Any(x => x.Equals(o.Name, StringComparison.InvariantCultureIgnoreCase)))
                    .ToList();

                return new GetCommandsByNamesResponse
                {
                    Commands = commands
                };
            }
        }

        private IEnumerable<string> ParseCommandNames(string names)
        {
            if (string.IsNullOrWhiteSpace(names))
                return new string[0];

            return names.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
