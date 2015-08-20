using AutoMapper;
using ReMi.BusinessEntities;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.Common.Utils.Repository;
using Role = ReMi.DataEntities.Auth.Role;

namespace ReMi.DataAccess.BusinessEntityGateways.Auth
{
    public class RoleGateway : BaseGateway, IRoleGateway
    {
        public IRepository<Account> AccountRepository { get; set; }
        public IRepository<Role> RoleRepository { get; set; }
        public IMappingEngine Mapper { get; set; }

        public IEnumerable<BusinessEntities.Auth.Role> GetRoles()
        {
            return RoleRepository.GetAllSatisfiedBy()
                .Select(x => Mapper.Map<Role, BusinessEntities.Auth.Role>(x))
                .ToList();
        }
        
        public void CreateRole(BusinessEntities.Auth.Role role)
        {
            if (role == null)
                throw new ArgumentNullException("role");

            var existingRole = RoleRepository.GetSatisfiedBy(o => o.ExternalId == role.ExternalId || o.Name == role.Name || o.Description == role.Description);
            if (existingRole != null)
                throw new RoleAlreadyExistsException(role.Name);

            var dataRole = Mapper.Map<BusinessEntities.Auth.Role, Role>(role);
            if (dataRole == null)
                throw new Exception("Can't convert role to data entity");

            Logger.DebugFormat("Creating new role: Role={0}", dataRole);

            RoleRepository.Insert(dataRole);
        }

        public void UpdateRole(BusinessEntities.Auth.Role role)
        {
            if (role == null)
                throw new ArgumentNullException("role");

            var existingRole = RoleRepository.GetSatisfiedBy(o => o.ExternalId == role.ExternalId);
            if (existingRole == null)
                throw new RoleNotFoundException(role.ExternalId);

            var similarRoles = RoleRepository.Entities.Any(o => o.ExternalId != role.ExternalId && (o.Name == role.Name || o.Description == role.Description));
            if (similarRoles)
                throw new RoleAlreadyExistsException(role.Name);

            var accountsAttached = AccountRepository.Entities.Any(o => o.Role.ExternalId == existingRole.ExternalId);
            if (accountsAttached)
                throw new Exception("Can't update role when attached accounts exists");

            //TODO: need to remove this after implemebntation of dynamic roles
            if (existingRole.Id <= 8)
                throw new Exception("Can't change base roles while dynamic permission management not implemented");

            Logger.DebugFormat("Updating role: Role={0}", role);

            RoleRepository.Update(
                row => row.ExternalId == role.ExternalId,
                row =>
                {
                    row.Name = role.Name;
                    row.Description = role.Description;
                });
        }

        public void DeleteRole(BusinessEntities.Auth.Role role)
        {
            if (role == null)
                throw new ArgumentNullException("role");

            var existingRole = RoleRepository.GetSatisfiedBy(o => o.ExternalId == role.ExternalId);
            if (existingRole == null)
                throw new RoleNotFoundException(role.ExternalId);

            var accountsAttached = AccountRepository.Entities.Any(o => o.Role.ExternalId == existingRole.ExternalId);
            if (accountsAttached)
                throw new Exception("Can't update role when attached accounts exists");

            //TODO: need to remove this after implemebntation of dynamic roles
            if (existingRole.Id <= 8)
                throw new Exception("Can't change base roles while dynamic permission management not implemented");

            Logger.DebugFormat("Deleting role: Role={0}", role);

            RoleRepository.Delete(existingRole);
        }

        public override void OnDisposing()
        {
            AccountRepository.Dispose();
            RoleRepository.Dispose();

            base.OnDisposing();
        }
    }
}
