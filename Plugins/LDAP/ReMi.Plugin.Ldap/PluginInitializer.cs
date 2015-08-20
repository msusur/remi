using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Linq;
using Autofac;
using Autofac.Core;
using AutoMapper;
using ReMi.Common.Utils.Repository;
using ReMi.Contracts.Plugins.Data;
using ReMi.Contracts.Plugins.Services;
using ReMi.Contracts.Plugins.Services.Authentication;
using ReMi.Plugin.Ldap.AutoMapper;
using ReMi.Plugin.Ldap.DataAccess;
using ReMi.Plugin.Ldap.Migrations;

namespace ReMi.Plugin.Ldap
{
    public class PluginInitializer : IPluginInitializer
    {
        private const string LdapKey = "ReMi.Plugin.Ldap";
        private const PluginType LdapPluginType = PluginType.Authentication;
        public const string LdapId = "DE2A5CAD-8CA7-4FC6-868B-B70DBF015A11";

        public string Key { get { return LdapKey; } }
        public Guid Id { get; private set; }
        public PluginType PluginType { get { return LdapPluginType; } }
        public bool IsGlobalConfigurationReadonly { get { return false; } }
        public bool IsPackageConfigurationReadonly { get { return false; } }

        public PluginInitializer()
        {
            Id = new Guid(LdapId);
        }

        public void InitializeDatabase()
        {
            var configuration = new Configuration();

            var migrator = new DbMigrator(configuration);
            migrator.Update();
        }

        public void InitializeDependencies(ContainerBuilder builder)
        {

            builder.RegisterType<PluginInitializer>()
                .As<IPluginInitializer>()
                .Named<IPluginInitializer>(LdapId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();

            builder.RegisterType<PluginConfiguration>()
                .As<IPluginConfiguration>()
                .As<IPluginConfiguration<PluginConfigurationEntity>>()
                .Named<IPluginConfiguration>(LdapId)
                .Named<IPluginConfiguration<PluginConfigurationEntity>>(LdapId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();


            var propertyResolver = Enumerable.Repeat(new ResolvedParameter(
                (p, c) => p.ParameterType.Namespace != null && p.ParameterType.Namespace.StartsWith("ReMi.Plugin.Ldap"),
                (p, c) => c.ResolveNamed(LdapId, p.ParameterType)), 2);
            builder.RegisterType<LdapRequest>()
                .As<IAuthenticationService>()
                .Named<IAuthenticationService>(LdapId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope()
                .WithProperties(propertyResolver);

            builder.RegisterType<EntityFrameworkUnitOfWork<LdapContext>>()
                .As<IUnitOfWork>()
                .Named<IUnitOfWork>(LdapId)
                .InstancePerDependency()
                .PreserveExistingDefaults()
                .PropertiesAutowired();

            builder.RegisterGeneric(typeof(EntityFrameworkRepository<>))
                .As(typeof(IRepository<>))
                .Named(LdapId, typeof(IRepository<>))
                .InstancePerDependency()
                .PropertiesAutowired()
                .WithParameter(new ResolvedParameter(
                    (p, c) => p.ParameterType.Name == typeof(IUnitOfWork).Name,
                    (p, c) => c.ResolveNamed<IUnitOfWork>(LdapId)));

            var repositoryPropertyResolver = Enumerable.Repeat(new ResolvedParameter(
                (p, c) => p.ParameterType.Name == typeof(IRepository<>).Name,
                (p, c) => c.ResolveNamed(LdapId, p.ParameterType)), 5);
            builder.RegisterAssemblyTypes(GetType().Assembly)
                .Where(t => !string.IsNullOrEmpty(t.Namespace) && t.Namespace.StartsWith("ReMi.Plugin.Ldap.DataAccess.Gateways"))
                .AsImplementedInterfaces()
                .InstancePerDependency()
                .PropertiesAutowired()
                .PreserveExistingDefaults()
                .WithProperties(repositoryPropertyResolver);
        }

        public IEnumerable<Profile> GetAutoMapperProfiles()
        {
            return new Profile[]
            {
                new LdapDataEntitiesToBusinessEnities(),
                new LdapBusinessEnitiesToDataEntities()
            };
        }
    }
}
