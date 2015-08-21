using AutoMapper;
using ReMi.DataEntities.Api;
using ReMi.DataEntities.Auth;
using System.Collections.Generic;
using System.Linq;
using ReMi.Common.Utils.Repository;
using ReMi.Contracts.Plugins.Data;
using ReMi.DataEntities.Plugins;

namespace ReMi.DataAccess.BusinessEntityGateways.Auth
{
    public class SecurityGateway : BaseGateway, ISecurityGateway
    {
        public IRepository<Command> CommandRepository { get; set; }
        public IRepository<Query> QueryRepository { get; set; }
        public IRepository<Account> AccountRepository { get; set; }
        public IRepository<PluginConfiguration> PluginConfigurationRepository { get; set; }

        public IMappingEngine Mapper { get; set; }

        public IEnumerable<BusinessEntities.Auth.Role> GetCommandRoles(string commandName)
        {
            return CommandRepository.GetSatisfiedBy(x => x.Name == commandName)
                .CommandPermissions
                .Select(x => Mapper.Map<Role, BusinessEntities.Auth.Role>(x.Role))
                .ToList();
        }

        public IEnumerable<BusinessEntities.Auth.Role> GetQueryRoles(string queryName)
        {
            return QueryRepository.GetSatisfiedBy(x => x.Name == queryName)
                .QueryPermissions
                .Select(x => Mapper.Map<Role, BusinessEntities.Auth.Role>(x.Role))
                .ToList();
        }

        public bool HasAuthentication()
        {
            return AccountRepository.Entities.Any()
                   && PluginConfigurationRepository.Entities.Any(
                       x => x.PluginType == PluginType.Authentication && x.PluginId.HasValue);
        }

        public override void OnDisposing()
        {
            CommandRepository.Dispose();
            QueryRepository.Dispose();
            AccountRepository.Dispose();
            PluginConfigurationRepository.Dispose();
        }
    }
}
