using System;
using AutoMapper;
using ReMi.BusinessEntities.Api;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.Auth;
using System.Collections.Generic;
using System.Linq;
using ReMi.Common.Utils.Repository;
using ReMi.Contracts.Plugins.Data;
using ReMi.DataEntities.Plugins;
using DataCommand = ReMi.DataEntities.Api.Command;

namespace ReMi.DataAccess.BusinessEntityGateways.Auth
{
    public class CommandPermissionsGateway : BaseGateway, ICommandPermissionsGateway
    {
        public IRepository<DataCommand> CommandRepository { get; set; }
        public IRepository<Role> RoleRepository { get; set; }
        public IRepository<CommandPermission> CommandPermissionRepository { get; set; }
        public IRepository<Account> AccountRepository { get; set; }
        public IRepository<PluginConfiguration> PluginConfigurationRepository { get; set; }
        public IMappingEngine Mapper { get; set; }

        public override void OnDisposing()
        {
            base.OnDisposing();
            CommandRepository.Dispose();
            RoleRepository.Dispose();
            CommandPermissionRepository.Dispose();
            AccountRepository.Dispose();
            PluginConfigurationRepository.Dispose();
        }

        public IEnumerable<Command> GetCommands(bool includeBackground = false)
        {
            return CommandRepository.GetAllSatisfiedBy(x => !x.IsBackground || includeBackground)
                .Select(x => Mapper.Map<DataCommand, Command>(x))
                .OrderBy(x => x.Group)
                .ThenBy(x => x.Name)
                .ToArray();
        }

        public IEnumerable<string> GetAllowedCommands(Guid? roleId)
        {
            if (!roleId.HasValue)
            {
                var hasAuthentication = AccountRepository.Entities.Any()
                   && PluginConfigurationRepository.Entities.Any(
                       x => x.PluginType == PluginType.Authentication && x.PluginId.HasValue);
                return hasAuthentication ? Enumerable.Empty<string>() : CommandRepository.Entities.Select(x => x.Name).ToArray();
            }
            var role = RoleRepository.GetSatisfiedBy(x => x.ExternalId == roleId);
            if (role == null)
                throw new EntityNotFoundException(typeof (Role), roleId);
            return role.Name == "Admin"
                ? CommandRepository.Entities.Select(x => x.Name).ToArray()
                : CommandPermissionRepository.GetAllSatisfiedBy(x => x.Role.ExternalId == roleId)
                    .Select(s => s.Command.Name).ToArray();
        }

        public void AddCommandPermission(int commandId, Guid roleExternalId)
        {
            var command = CommandRepository.GetSatisfiedBy(x => x.CommandId == commandId);
            if (command == null)
                throw new CommandNotFoundException(commandId);
            var role = RoleRepository.GetSatisfiedBy(x => x.ExternalId == roleExternalId);
            if (role == null)
                throw new RoleNotFoundException(roleExternalId);

            if (command.CommandPermissions == null || command.CommandPermissions.Any(x => x.RoleId == role.Id))
                return;

            command.CommandPermissions.Add(new CommandPermission
            {
                CommandId = command.CommandId,
                RoleId = role.Id
            });
            CommandRepository.Update(command);
        }

        public void RemoveCommandPermission(int commandId, Guid roleExternalId)
        {
            var command = CommandRepository.GetSatisfiedBy(x => x.CommandId == commandId);
            if (command == null)
                throw new CommandNotFoundException(commandId);
            var role = RoleRepository.GetSatisfiedBy(x => x.ExternalId == roleExternalId);
            if (role == null)
                throw new RoleNotFoundException(roleExternalId);

            if (command.CommandPermissions == null || command.CommandPermissions.All(x => x.RoleId != role.Id))
                return;

            CommandPermissionRepository.Delete(CommandPermissionRepository
                .GetSatisfiedBy(x => x.RoleId == role.Id && x.CommandId == commandId));
        }
    }
}
