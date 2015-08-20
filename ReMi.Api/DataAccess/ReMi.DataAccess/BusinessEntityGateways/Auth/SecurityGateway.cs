using AutoMapper;
using ReMi.DataEntities.Api;
using ReMi.DataEntities.Auth;
using System.Collections.Generic;
using System.Linq;
using ReMi.Common.Utils.Repository;

namespace ReMi.DataAccess.BusinessEntityGateways.Auth
{
    public class SecurityGateway : BaseGateway, ISecurityGateway
    {
        public IRepository<Command> CommandRepository { get; set; }
        public IRepository<Query> QueryRepository { get; set; }

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

        public override void OnDisposing()
        {
            CommandRepository.Dispose();
            QueryRepository.Dispose();
        }
    }
}
