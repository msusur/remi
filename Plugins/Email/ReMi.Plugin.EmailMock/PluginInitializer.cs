using Autofac;
using Autofac.Core;
using AutoMapper;
using ReMi.Contracts.Plugins.Data;
using ReMi.Contracts.Plugins.Services;
using ReMi.Contracts.Plugins.Services.Email;
using ReMi.Plugin.EmailMock.AutoMapper;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using ReMi.Common.Utils.Repository;
using ReMi.Plugin.EmailMock.DataAccess;
using ReMi.Plugin.EmailMock.Migrations;

namespace ReMi.Plugin.EmailMock
{
    public class PluginInitializer : IPluginInitializer
    {
        private const string EmailKey = "ReMi.Plugin.EmailMock";
        private const PluginType EmailPluginType = PluginType.Email;
        public const string EmailId = "8133B49C-DD6A-415E-83EE-D58507F32C9A";

        public string Key { get { return EmailKey; } }
        public Guid Id { get; private set; }
        public PluginType PluginType { get { return EmailPluginType; } }
        public bool IsGlobalConfigurationReadonly { get { return false; } }
        public bool IsPackageConfigurationReadonly { get { return true; } }

        public PluginInitializer()
        {
            Id = new Guid(EmailId);
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
                .Named<IPluginInitializer>(EmailId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();

            builder.RegisterType<PluginConfiguration>()
                .As<IPluginConfiguration>()
                .As<IPluginConfiguration<PluginConfigurationEntity>>()
                .Named<IPluginConfiguration>(EmailId)
                .Named<IPluginConfiguration<PluginConfigurationEntity>>(EmailId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();

            builder.RegisterType<EmailServiceMock>()
                .As<IEmailService>()
                .Named<IEmailService>(EmailId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope()
                .WithProperty(new ResolvedParameter(
                    (p, c) =>
                        p.ParameterType.Namespace != null &&
                        p.ParameterType.Namespace.StartsWith("ReMi.Plugin.EmailMock"),
                    (p, c) => c.ResolveNamed(EmailId, p.ParameterType)));

            builder.RegisterType<EntityFrameworkUnitOfWork<EmailMockContext>>()
                .As<IUnitOfWork>()
                .Named<IUnitOfWork>(EmailId)
                .InstancePerDependency()
                .PreserveExistingDefaults()
                .PropertiesAutowired();

            builder.RegisterGeneric(typeof(EntityFrameworkRepository<>))
                .As(typeof(IRepository<>))
                .Named(EmailId, typeof(IRepository<>))
                .InstancePerDependency()
                .PropertiesAutowired()
                .WithParameter(new ResolvedParameter(
                    (p, c) => p.ParameterType.Name == typeof(IUnitOfWork).Name,
                    (p, c) => c.ResolveNamed<IUnitOfWork>(EmailId)));

            var repositoryPropertyResolver = Enumerable.Repeat(new ResolvedParameter(
                (p, c) => p.ParameterType.Name == typeof(IRepository<>).Name,
                (p, c) => c.ResolveNamed(EmailId, p.ParameterType)), 5);
            builder.RegisterAssemblyTypes(GetType().Assembly)
                .Where(t => !string.IsNullOrEmpty(t.Namespace) && t.Namespace.StartsWith("ReMi.Plugin.EmailMock.DataAccess.Gateways"))
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
                new EmailMockDataEntitiesToBusinessEnities(),
                new EmailMockBusinessEnitiesToDataEntities()
            };
        }
    }
}
