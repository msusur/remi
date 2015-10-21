using Autofac;
using Autofac.Core;
using Autofac.Integration.WebApi;
using AutoMapper;
using Common.Logging;
using ReMi.Api.Insfrastructure;
using ReMi.Api.Insfrastructure.Commands;
using ReMi.Api.Insfrastructure.Notifications;
using ReMi.Api.Insfrastructure.Notifications.Filters;
using ReMi.Api.Insfrastructure.Queries;
using ReMi.Api.Insfrastructure.Security;
using ReMi.BusinessLogic.Api;
using ReMi.CommandHandlers.ExecPoll;
using ReMi.Common.Utils;
using ReMi.Common.Utils.Repository;
using ReMi.Common.WebApi;
using ReMi.Common.WebApi.Notifications;
using ReMi.Common.WebApi.Tracking;
using ReMi.Contracts.Cqrs;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess;
using ReMi.DataAccess.Helpers;
using ReMi.EventHandlers;
using ReMi.Plugin.Common.PluginsConfiguration;
using ReMi.Plugin.Composites.Services;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Dependencies;

namespace ReMi.Api
{
    public static class DependencyResolverConfig
    {
        private const string RemiKey = "C5BC8B6B-28F0-4444-A04F-D86D11EAB9D8";
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();

        public static void Configure(HttpConfiguration configuration)
        {
            configuration.DependencyResolver =
                new AutofacWebApiDependencyResolver(RegisterDependencies(new ContainerBuilder(), configuration));
        }

        private static IContainer RegisterDependencies(ContainerBuilder builder, HttpConfiguration configuration)
        {
            // Register the Web API controllers.
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly())
                .PropertiesAutowired()
                .InstancePerDependency();

            // Register other dependencies.			
            var assemblies = RemiAssembliesHelper.GetReMiAssemblies();

            RegisterCqrs(builder, assemblies);

            RegisterBusinessLogic(builder, assemblies);

            RegisterDataAccess(builder, assemblies);

            RegisterMappingEngine(builder, assemblies);

            RegisterEvents(builder, assemblies);

            RegisterNotifications(builder, assemblies);

            RegisterOther(builder);

            RegisterPlugins(builder);

            builder.RegisterWebApiFilterProvider(configuration);

            const string containerKey = "Container";
            var tempDic = new Dictionary<string, IContainer> { { containerKey, null } };
            builder.Register(x => tempDic[containerKey]).As<IContainer>().SingleInstance();

            tempDic[containerKey] = builder.Build();

            return tempDic[containerKey];
        }

        private static void RegisterCqrs(ContainerBuilder builder, Assembly[] assemblies)
        {
            builder.RegisterAssemblyTypes(assemblies)
                .AsClosedTypesOf(typeof(IValidateRequest<>))
                .InstancePerDependency()
                .PropertiesAutowired();

            builder.RegisterAssemblyTypes(assemblies)
                .AsClosedTypesOf(typeof(IHandleQuery<,>))
                .InstancePerDependency().PropertiesAutowired();

            builder.RegisterAssemblyTypes(assemblies)
                .AsClosedTypesOf(typeof(IHandleCommand<>))
                .InstancePerDependency().PropertiesAutowired();

            builder.RegisterGeneric(typeof(QueryActionImplementation<,>))
                .As(typeof(IImplementQueryAction<,>))
                .PropertiesAutowired()
                .InstancePerDependency();

            builder.RegisterGeneric(typeof(CommandProcessorGeneric<>))
                .As(typeof(ICommandProcessorGeneric<>))
                .PropertiesAutowired()
                .InstancePerDependency();

            builder.RegisterType(typeof(CommandTrackerHandler))
                .As(typeof(ICommandTracker))
                .InstancePerDependency();

            builder.RegisterType(typeof(CommandProcessor))
                .As(typeof(ICommandProcessor))
                .InstancePerLifetimeScope()
                .PropertiesAutowired();

            builder.Register(c => new CommandDispatcher())
                .As(typeof(ICommandDispatcher))
                .SingleInstance()
                .PropertiesAutowired();

            builder.RegisterAssemblyTypes(assemblies)
                .As(typeof(ICommand))
                .InstancePerLifetimeScope();

            builder.RegisterType(typeof(AuthorizationManager))
                .As(typeof(IAuthorizationManager))
                .PropertiesAutowired()
                .InstancePerLifetimeScope();

            builder.RegisterType(typeof(PrincipalSetter))
                .As(typeof(IPrincipalSetter))
                .InstancePerLifetimeScope();

            builder.RegisterType(typeof(PermissionChecker))
                .As(typeof(IPermissionChecker))
                .InstancePerLifetimeScope()
                .PropertiesAutowired();
        }

        private static void RegisterEvents(ContainerBuilder builder, Assembly[] assemblies)
        {
            builder.RegisterAssemblyTypes(assemblies)
                .AsClosedTypesOf(typeof(IHandleEvent<>))
                .InstancePerDependency()
                .PropertiesAutowired();

            builder.Register(c => new EventPublisher(c.Resolve<IDependencyResolver>()))
               .As<IPublishEvent>()
               .SingleInstance()
               .PropertiesAutowired();

            builder.RegisterType(typeof(EventTrackerHandler))
                .As(typeof(IEventTracker))
                .InstancePerDependency();
        }

        private static void RegisterBusinessLogic(ContainerBuilder builder, Assembly[] assemblies)
        {
            builder.RegisterAssemblyTypes(assemblies)
                .Where(t => !string.IsNullOrWhiteSpace(t.Namespace) && t.Namespace.StartsWith("ReMi.BusinessLogic"))
                .AsImplementedInterfaces()
                .InstancePerDependency()
                .PropertiesAutowired();

            builder.Register<IApiDescriptionBuilder>(
                c => new ApiDescriptionBuilder(
                    assemblies.SelectMany(a => a.GetTypes().Where(t => typeof(ApiController).IsAssignableFrom(t)).Select(x => x)),
                    assemblies.SelectMany(a => a.GetTypes().Where(t => typeof(ICommand).IsAssignableFrom(t) && t.IsClass).Select(x => x))
                    ))
                .PropertiesAutowired()
                .InstancePerDependency();
        }

        private static void RegisterDataAccess(ContainerBuilder builder, Assembly[] assemblies)
        {
            builder.RegisterType<EntityFrameworkUnitOfWork<ReleaseContext>>()
                .As<IUnitOfWork>()
                .Named<IUnitOfWork>(RemiKey)
                .InstancePerDependency()
                .PropertiesAutowired();

            builder.RegisterGeneric(typeof(EntityFrameworkRepository<>))
                .As(typeof(IRepository<>))
                .Named(RemiKey, typeof(IRepository<>))
                .InstancePerDependency()
                .PropertiesAutowired()
                .WithParameter(new ResolvedParameter(
                    (p, c) => p.ParameterType.Name == typeof(IUnitOfWork).Name,
                    (p, c) => c.ResolveNamed<IUnitOfWork>(RemiKey)));

            builder.RegisterType(typeof(DatabaseAdapter))
               .As(typeof(IDatabaseAdapter))
               .InstancePerDependency()
               .PropertiesAutowired()
               .WithProperty(new ResolvedParameter(
                    (p, c) => p.ParameterType.Name == typeof(IUnitOfWork).Name,
                    (p, c) => c.ResolveNamed<IUnitOfWork>(RemiKey)));

            var repositoryPropertyResolver = Enumerable.Repeat(new ResolvedParameter(
                (p, c) => p.ParameterType.Name == typeof (IRepository<>).Name,
                (p, c) => c.ResolveNamed(RemiKey, p.ParameterType)), 20);
            builder.RegisterAssemblyTypes(assemblies)
                .Where(t => !string.IsNullOrWhiteSpace(t.Namespace) && t.Namespace.StartsWith("ReMi.DataAccess.BusinessEntityGateways"))
                .AsImplementedInterfaces()
                .InstancePerDependency()
                .PropertiesAutowired()
                .WithProperties(repositoryPropertyResolver);

        }

        private static void RegisterMappingEngine(ContainerBuilder builder, Assembly[] assemblies)
        {
            builder.Register(c => Mapper.Engine).As<IMappingEngine>();

            builder.RegisterAssemblyTypes(assemblies)
                .AssignableTo<Profile>()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(assemblies)
                .AsClosedTypesOf(typeof(ITypeConverter<,>))
                .InstancePerDependency()
                .PropertiesAutowired();
        }

        private static void RegisterNotifications(ContainerBuilder builder, Assembly[] assemblies)
        {
            builder.Register(c => new SubscriptionManager()).As<ISubscriptionManager>()
                .PropertiesAutowired()
                .InstancePerDependency();

            builder.Register(c => new NotificationsHub(c.Resolve<ISerialization>(), c.Resolve<ISubscriptionManager>()))
                .As<IFrontendNotificator>()
                .PropertiesAutowired()
                .InstancePerDependency();

            builder.Register(c => new NotificationFilterApplying())
                .As<INotificationFilterApplying>()
                .SingleInstance()
                .PropertiesAutowired();


            builder.RegisterAssemblyTypes(assemblies)
                .AsClosedTypesOf(typeof(INotificationFilterApplication<>))
                .PropertiesAutowired()
                .InstancePerDependency();
        }

        private static void RegisterOther(ContainerBuilder builder)
        {
            builder.Register(c => new Serialization()).As<ISerialization>().InstancePerLifetimeScope();

            builder.Register(c => GlobalConfiguration.Configuration.DependencyResolver).As<IDependencyResolver>().SingleInstance();

            builder.Register(c => new FileStorage(HostingEnvironment.MapPath("~"))).As<IFileStorage>().SingleInstance();

            builder.Register(c => new FileStorage(HostingEnvironment.MapPath("~/App_Data"))).As<IAppDataFileStorage>().SingleInstance();

            builder.Register(c => new ApplicationSettings()).As<IApplicationSettings>()
                .SingleInstance()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            builder.Register(c => new ClientRequestInfoRetriever()).As<IClientRequestInfoRetriever>().SingleInstance();
        }

        private static void RegisterPlugins(ContainerBuilder builder)
        {
            Logger.Info("Registering Plugin Dependencies");

            builder.RegisterType<CheckQaStatusComposite>()
                .AsImplementedInterfaces()
                .PropertiesAutowired()
                .SingleInstance();

            builder.RegisterType<ReleaseContentComposite>()
                .AsImplementedInterfaces()
                .PropertiesAutowired()
                .SingleInstance();

            builder.RegisterType<DeploymentToolComposite>()
                .AsImplementedInterfaces()
                .PropertiesAutowired()
                .SingleInstance();

            builder.RegisterType<SourceControlComposite>()
                .AsImplementedInterfaces()
                .PropertiesAutowired()
                .SingleInstance();

            builder.RegisterType<AuthenticationServiceComposite>()
                .AsImplementedInterfaces()
                .PropertiesAutowired()
                .SingleInstance();

            builder.RegisterType<EmailServiceComposite>()
                .AsImplementedInterfaces()
                .PropertiesAutowired()
                .SingleInstance();

            builder.RegisterType<HelpDeskServiceComposite>()
                .AsImplementedInterfaces()
                .PropertiesAutowired()
                .SingleInstance();

            builder.RegisterType<CacheServiceComposite>()
                .AsImplementedInterfaces()
                .PropertiesAutowired()
                .SingleInstance();

            var pluginConfiguration = new PluginConfiguration();
            builder.Register<IPluginConfiguration>(x => pluginConfiguration)
                .SingleInstance();

            foreach (var pluginInitializer in pluginConfiguration.PluginInitializers)
            {
                pluginInitializer.InitializeDependencies(builder);
            }
        }
    }
}
