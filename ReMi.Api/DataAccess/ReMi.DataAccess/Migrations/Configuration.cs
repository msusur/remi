using System.Data.Entity.Migrations;
using ReMi.Common.Constants.BusinessRules;
using ReMi.Common.Constants.ProductRequests;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Constants.ReleaseExecution;
using ReMi.Common.Constants.ReleasePlan;
using ReMi.Common.Constants.Subscriptions;
using ReMi.DataAccess.Helpers;
using ReMi.DataEntities.Api;
using ReMi.DataEntities.BusinessRules;
using ReMi.DataEntities.Metrics;
using ReMi.DataEntities.ProductRequests;
using ReMi.DataEntities.ReleaseCalendar;
using ReMi.DataEntities.ReleasePlan;
using ReMi.DataEntities.Subscriptions;
using System.Collections.Generic;
using ReMi.Common.Utils.Repository;
using ReMi.Contracts.Plugins.Data;
using ReMi.DataEntities.Auth;
using ReMi.DataEntities.Plugins;

namespace ReMi.DataAccess.Migrations
{
    using DataEntities.Evt;
    using DataEntities.ExecPoll;
    using System;
    using System.Linq;

    public sealed class Configuration : DbMigrationsConfiguration<ReleaseContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "ReMi.DataAccess.ReleaseContext";
            SetHistoryContextFactory("System.Data.SqlClient",
                (conn, schema) => new ReMiMigrationHistoryContext(conn));
        }

        protected override void Seed(ReleaseContext context)
        {
            //This method will be called after migrating to the latest version.

            context.CommandStateTypes.AddOrUpdate(
                ct => ct.Description,
                Enum.GetNames(typeof(CommandStateType))
                .Select(ct => new CommandStateTypeDescription { Description = ct })
                .ToArray()
                );

            context.EventStateTypes.AddOrUpdate(
                ct => ct.Description,
                Enum.GetNames(typeof(EventStateType))
                .Select(ct => new EventStateTypeDescription { Description = ct })
                .ToArray()
                );

            context.ReleaseTaskTypes.AddOrUpdateEnum<ReleaseTaskType, ReleaseTaskTypeDescription>();
            context.TicketRisk.AddOrUpdateEnum<TicketRisk, TicketRiskDescription>();
            context.ReleaseTypes.AddOrUpdateEnum<ReleaseType, ReleaseTypeDescription>();
            context.ReleaseDecision.AddOrUpdateEnum<ReleaseDecision, ReleaseDecisionDescription>();
            context.MetricTypes.AddOrUpdateOrderedEnum<MetricType, MetricTypeDescription>();
            context.ReleaseTrack.AddOrUpdateEnum<ReleaseTrack, ReleaseTrackDescription>();
            context.BusinessRuleGroup.AddOrUpdateEnum<BusinessRuleGroup, BusinessRuleGroupDescription>();
            context.NotificationDescriptions.AddOrUpdateEnum<NotificationType, NotificationTypeDescription>();
            context.RemovingReasons.AddOrUpdateEnum<RemovingReason, RemovingReasonDescription>();
            context.PluginTypes.AddOrUpdateEnum<PluginType, PluginTypeDescription>();

            InitialiseEmptyDb(context);
            InitialiseQueries(context);
            InitialiseCommands(context);
            InitialiseBusinessRules(context);
            InitialisePlugins(context);
        }

        private static void InitialiseEmptyDb(ReleaseContext context)
        {
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(new []
                {
                    new Role { Description = "Administrator", ExternalId = Guid.NewGuid(), Name = "Admin" },
                    new Role { Description = "Basic User", ExternalId = Guid.NewGuid(), Name = "BasicUser" },
                    new Role { Description = "Not Authenticated", ExternalId = Guid.NewGuid(), Name = "NotAuthenticated" }
                });
            }

            if (context.CommandPermissions.Any()) return;

            var command = new Command
            {
                Name = "StartSessionCommand",
                Description = "StartSessionCommand",
                Group = "StartSessionCommand",
                IsBackground = false,
                Namespace = "StartSessionCommand"
            };
            context.Commands.AddOrUpdate(x => x.Name, command);
            context.SaveChanges();

            var commandId = context.Commands.First(x => x.Name == "StartSessionCommand").CommandId;
            var roleId = context.Roles.First(x => x.Name == "NotAuthenticated").Id;
            context.CommandPermissions.Add(new CommandPermission
            {
                CommandId = commandId,
                RoleId = roleId
            });
        }

        private static void InitialiseQueries(ReleaseContext context)
        {
            var queries = QueryCollector.Collect();
            var dbQueries = context.Queries.ToArray();

            if (dbQueries.Any(x => queries.All(c => c.Name != x.Name)))
            {
                // if there is command which not exists any more throw exception
                var nonExistingQueries = dbQueries.Where(x => queries.All(c => c.Name != x.Name)).ToArray();
                context.Queries.RemoveRange(nonExistingQueries);
            }
            dbQueries = context.Queries.ToArray();
            context.Queries.AddOrUpdate(x => x.Name, queries.Select(x => new Query
            {
                Name = x.Name,
                Description = x.Description,
                Group = x.Group,
                IsStatic = x.IsStatic,
                Namespace = x.Namespace,
                RuleId = dbQueries.Any(c => c.Name == x.Name && c.RuleId.HasValue) ? dbQueries.Single(c => c.Name == x.Name).RuleId : (int?)null
            }).ToArray());
        }

        private static void InitialiseCommands(ReleaseContext context)
        {
            var commands = CommandCollector.Collect();
            var dbCommands = context.Commands.ToArray();

            if (dbCommands.Any(x => commands.All(c => c.Name != x.Name)))
            {
                var nonExistingCommands = dbCommands.Where(x => commands.All(c => c.Name != x.Name)).ToArray();
                context.Commands.RemoveRange(nonExistingCommands);
            }
            dbCommands = context.Commands.ToArray();
            context.Commands.AddOrUpdate(x => x.Name, commands.Select(x => new Command
            {
                Name = x.Name,
                Description = x.Description,
                Group = x.Group,
                IsBackground = x.IsBackground,
                Namespace = x.Namespace,
                RuleId = dbCommands.Any(c => c.Name == x.Name && c.RuleId.HasValue) ? dbCommands.Single(c => c.Name == x.Name).RuleId : (int?)null
            }).ToArray());
        }

        private static void InitialiseBusinessRules(ReleaseContext context)
        {
            context.SaveChanges();
            var rules = (IList<BusinessRuleDescription>)BusinessRuleCollector.Collect();
            var dbRules = context.BusinessRule
                .Where(x => x.Group != BusinessRuleGroup.Permissions)
                .ToArray();

            if (dbRules.Any(x => rules.All(c => c.ExternalId != x.ExternalId)))
            {
                var nonExistingRules = dbRules.Where(x => rules.All(c => c.ExternalId != x.ExternalId)).ToArray();
                context.BusinessRule.RemoveRange(nonExistingRules);
            }
            context.BusinessRule.AddOrUpdate(x => x.ExternalId, rules.Select(x => new BusinessRuleDescription
            {
                Name = x.Name,
                Description = x.Description,
                ExternalId = x.ExternalId,
                Group = x.Group,
                Script =
                    dbRules.Any(r => r.ExternalId == x.ExternalId)
                        ? dbRules.First(r => r.ExternalId == x.ExternalId).Script
                        : string.Empty
            }).ToArray());
            context.SaveChanges();

            dbRules = context.BusinessRule
                .Where(x => x.Group != BusinessRuleGroup.Permissions)
                .ToArray();

            var emptyAccountTestData = dbRules.Where(x => x.AccountTestData == null).ToList();
            emptyAccountTestData.ForEach(x =>
            {
                context.BusinessRuleAccountTestData.Add(new BusinessRuleAccountTestData
                {
                    Rule = x,
                    ExternalId = Guid.NewGuid(),
                    JsonData = "{ }"
                });
            });
            var parameters = rules.Where(x => x.Parameters != null).SelectMany(x => x.Parameters).ToList();
            var dbParams = dbRules.Where(x => x.Parameters != null).SelectMany(x => x.Parameters).ToList();
            if (dbParams.Any(x => parameters.All(c => c.ExternalId != x.ExternalId)))
            {
                var nonExistingParams = dbParams.Where(x => parameters.All(c => c.ExternalId != x.ExternalId)).ToArray();
                context.BusinessRuleParameter.RemoveRange(nonExistingParams);
                dbParams.RemoveAll(nonExistingParams.Contains);
            }

            parameters.ForEach(x => x.BusinessRuleId
                = dbRules.Single(r => r.ExternalId == x.BusinessRule.ExternalId).BusinessRuleId);
            dbParams.ForEach(x =>
            {
                if (x.TestData == null)
                    context.BusinessRuleTestData.Add(new BusinessRuleTestData
                    {
                        BusinessRuleTestDataId = x.BusinessRuleParameterId,
                        ExternalId = Guid.NewGuid(),
                        JsonData = string.Empty
                    });
            });

            context.BusinessRuleParameter.AddOrUpdate(x => x.ExternalId,
                parameters.Select(x => new BusinessRuleParameter
                {
                    Name = x.Name,
                    Type = x.Type,
                    ExternalId = x.ExternalId,
                    BusinessRuleId = x.BusinessRuleId,
                    TestData = x.TestData ?? new BusinessRuleTestData
                    {
                        BusinessRuleTestDataId = x.BusinessRuleParameterId,
                        ExternalId = Guid.NewGuid(),
                        JsonData = string.Empty
                    }
                }).ToArray());
        }

        private static void InitialisePlugins(ReleaseContext context)
        {
            var plugins = PluginCollector.Collect();
            if (!plugins.Any())
                return;

            var dbPlugins = context.Plugins.ToArray();
            // remove no longer existing plugins
            if (dbPlugins.Any(x => plugins.All(c => c.ExternalId != x.ExternalId)))
            {
                var nonExisting = dbPlugins.Where(x => plugins.All(c => c.ExternalId != x.ExternalId)).ToArray();
                context.Plugins.RemoveRange(nonExisting);
            }
            context.Plugins.AddOrUpdate(x => x.ExternalId, plugins.ToArray());

            var globalTypes = PluginCollector.CollectGlobalPluginTypes();
            var dbGlobalTypes = context.PluginConfiguration.ToArray();
            if (dbGlobalTypes.Any(x => globalTypes.All(c => x.PluginType != c)))
            {
                var nonExisting = dbGlobalTypes.Where(x => globalTypes.All(c => c != x.PluginType)).ToArray();
                context.PluginConfiguration.RemoveRange(nonExisting);
            }

            dbGlobalTypes = context.PluginConfiguration.ToArray();
            context.PluginConfiguration.AddOrUpdate(x => x.ExternalId, globalTypes
                .Select(x => new PluginConfiguration
                {
                    PluginType = x,
                    PluginId = dbGlobalTypes.Any(c => c.PluginType == x) ? dbGlobalTypes.First(c => c.PluginType == x).PluginId : (int?)null,
                    ExternalId = dbGlobalTypes.Any(c => c.PluginType == x) ? dbGlobalTypes.First(c => c.PluginType == x).ExternalId : Guid.NewGuid()
                }).ToArray());

            var packageTypes = PluginCollector.CollectPackagePluginTypes();
            var dbPackageTypes = context.PluginPackageConfiguration.ToArray();
            var packages = context.Products.ToArray();
            if (dbPackageTypes.Any(x => packageTypes.All(c => x.PluginType != c)))
            {
                var nonExisting = dbPackageTypes.Where(x => packageTypes.All(c => c != x.PluginType)).ToArray();
                context.PluginPackageConfiguration.RemoveRange(nonExisting);
            }
            if (dbPackageTypes.Any(x => packages.All(c => x.PackageId != c.ProductId)))
            {
                var nonExisting = dbPackageTypes.Where(x => packages.All(c => x.PackageId != c.ProductId)).ToArray();
                context.PluginPackageConfiguration.RemoveRange(nonExisting);
            }

            var packageConfigs = packageTypes
                .SelectMany(t1 => packages
                    .Select(t2 => new { PluginType = t1, Package = t2 }));
            dbPackageTypes = context.PluginPackageConfiguration.ToArray();
            context.PluginPackageConfiguration.AddOrUpdate(x => x.ExternalId, packageConfigs
                .Select(x =>
                {
                    var currentPackageConfig = dbPackageTypes.FirstOrDefault(
                            c => c.PluginType == x.PluginType && c.PackageId == x.Package.ProductId);
                    return new PluginPackageConfiguration
                    {
                        PluginType = x.PluginType,
                        PackageId = x.Package.ProductId,
                        PluginId = currentPackageConfig == null ? (int?)null : currentPackageConfig.PluginId,
                        ExternalId = currentPackageConfig == null ? Guid.NewGuid() : currentPackageConfig.ExternalId,
                    };
                }).ToArray());

        }
    }
}
