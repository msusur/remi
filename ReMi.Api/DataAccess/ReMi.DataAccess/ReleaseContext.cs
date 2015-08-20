using ReMi.DataAccess.Migrations;
using ReMi.DataEntities;
using ReMi.DataEntities.Api;
using ReMi.DataEntities.Auth;
using ReMi.DataEntities.BusinessRules;
using ReMi.DataEntities.Evt;
using ReMi.DataEntities.ExecPoll;
using ReMi.DataEntities.Metrics;
using ReMi.DataEntities.ProductRequests;
using ReMi.DataEntities.Products;
using ReMi.DataEntities.ReleaseCalendar;
using ReMi.DataEntities.ReleasePlan;
using ReMi.DataEntities.Reports;
using ReMi.DataEntities.Subscriptions;
using ReMi.DataEntityMaps;
using ReMi.DataEntityMaps.Auth;
using ReMi.DataEntityMaps.BusinessRules;
using ReMi.DataEntityMaps.Evt;
using ReMi.DataEntityMaps.ExecPoll;
using ReMi.DataEntityMaps.ProductRequests;
using ReMi.DataEntityMaps.Products;
using ReMi.DataEntityMaps.ReleaseCalendar;
using ReMi.DataEntityMaps.ReleasePlan;
using System.Data.Entity;
using ReMi.DataEntities.Plugins;
using ReMi.DataEntities.SourceControl;

namespace ReMi.DataAccess
{
    public class ReleaseContext : DbContext
    {
        public ReleaseContext()
            : base("ReleaseManagementConnection")
        {
        }

        public ReleaseContext(string connString)
            : base(connString)
        {
        }

        public DbSet<ReleaseWindow> ReleaseWindows { get; set; }
        public DbSet<ReleaseNote> ReleaseNotes { get; set; }
        public DbSet<ReleaseDecisionDescription> ReleaseDecision { get; set; }
        public DbSet<ReleaseParticipant> ReleaseParticipants { get; set; }
        public DbSet<ReleaseTask> ReleaseTasks { get; set; }
        public DbSet<ReleaseTaskAttachment> ReleaseTaskAttachments { get; set; }
        public DbSet<ReleaseTaskRiskDescription> ReleaseTaskRisks { get; set; }
        public DbSet<ReleaseTaskTypeDescription> ReleaseTaskTypes { get; set; }
        public DbSet<ReleaseTaskEnvironmentDescription> ReleaseTaskEnvironments { get; set; }
        public DbSet<ReleaseProduct> ReleaseProducts { get; set; }

        public DbSet<Product> Products { get; set; }
        public DbSet<ReleaseTypeDescription> ReleaseTypes { get; set; }
        public DbSet<ReleaseTrackDescription> ReleaseTrack { get; set; }

        public DbSet<CommandExecution> CommandExecutions { get; set; }
        public DbSet<CommandHistory> CommandHistory { get; set; }
        public DbSet<CommandStateTypeDescription> CommandStateTypes { get; set; }

        public DbSet<CheckListQuestion> CheckListQuestions { get; set; }
        public DbSet<CheckList> CheckList { get; set; }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<AccountProduct> AccountProducts { get; set; }
        public DbSet<MetricTypeDescription> MetricTypes { get; set; }
        public DbSet<ReleaseContent> ReleaseContent { get; set; }
        public DbSet<TicketRiskDescription> TicketRisk { get; set; }

        public DbSet<Event> Events { get; set; }
        public DbSet<EventHistory> EventHistory { get; set; }
        public DbSet<EventStateTypeDescription> EventStateTypes { get; set; }

        public DbSet<Command> Commands { get; set; }
        public DbSet<CommandPermission> CommandPermissions { get; set; }
        public DbSet<Query> Queries { get; set; }
        public DbSet<QueryPermission> QueryPermissions { get; set; }

        public DbSet<SourceControlChange> SourceControlChanges { get; set; }
        public DbSet<SourceControlChangeToReleaseWindow> SourceControlChangesToReleaseWindows { get; set; }

        public DbSet<ReleaseJob> ReleaseJobs { get; set; }
        public DbSet<ReleaseDeploymentMeasurement> ReleaseDeploymentMeasurements { get; set; }

        public DbSet<Description> Descriptions { get; set; }

        public DbSet<BusinessRuleDescription> BusinessRule { get; set; }
        public DbSet<BusinessRuleParameter> BusinessRuleParameter { get; set; }
        public DbSet<BusinessRuleGroupDescription> BusinessRuleGroup { get; set; }
        public DbSet<BusinessRuleTestData> BusinessRuleTestData { get; set; }
        public DbSet<BusinessRuleAccountTestData> BusinessRuleAccountTestData { get; set; }

        public DbSet<ProductRequestType> ProductRequestTypes { get; set; }
        public DbSet<ProductRequestGroup> ProductRequestGroups { get; set; }
        public DbSet<ProductRequestTask> ProductRequestTasks { get; set; }

        public DbSet<AccountNotification> AccountNotifications { get; set; }
        public DbSet<NotificationTypeDescription> NotificationDescriptions { get; set; }

        public DbSet<ProductRequestRegistration> ProductRequestRegistrations { get; set; }
        public DbSet<ProductRequestRegistrationTask> ProductRequestRegistrationTasks { get; set; }
        public DbSet<RemovingReasonDescription> RemovingReasons { get; set; }

        public DbSet<ReportDescription> ReportDescriptions { get; set; }
        public DbSet<ReportColumn> ReportColumns { get; set; }
        public DbSet<ReportParameter> ReportParameters { get; set; }

        public DbSet<DataEntities.Plugins.Plugin> Plugins { get; set; }
        public DbSet<PluginConfiguration> PluginConfiguration { get; set; }
        public DbSet<PluginPackageConfiguration> PluginPackageConfiguration { get; set; }
        public DbSet<PluginTypeDescription> PluginTypes { get; set; }

        public DbSet<ReleaseRepository> ReleaseReposities { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new ProductMap());
            modelBuilder.Configurations.Add(new ReleaseWindowMap());
            modelBuilder.Configurations.Add(new ReleaseNotesMap());
            modelBuilder.Configurations.Add(new ReleaseParticipantMap());
            modelBuilder.Configurations.Add(new ReleaseApproverMap());

            modelBuilder.Configurations.Add(new CommandExecutionMap());
            modelBuilder.Configurations.Add(new CommandHistoryMap());
            modelBuilder.Configurations.Add(new CommandStateTypeDescriptionMap());

            modelBuilder.Configurations.Add(new CheckListQuestionMap());
            modelBuilder.Configurations.Add(new CheckListMap());

            modelBuilder.Configurations.Add(new AccountMap());
            modelBuilder.Configurations.Add(new SessionMap());
            modelBuilder.Configurations.Add(new AccountProductMap());

            modelBuilder.Configurations.Add(new ReleaseTaskMap());
            modelBuilder.Configurations.Add(new ReleaseTaskAttachmentMap());


            modelBuilder.Configurations.Add(new EventMap());
            modelBuilder.Configurations.Add(new EventHistoryMap());
            modelBuilder.Configurations.Add(new EventStateTypeDescriptionMap());

            modelBuilder.Configurations.Add(new CheckListQuestionsToProductsMap());

            modelBuilder.Configurations.Add(new ReleaseContentMap());

            modelBuilder.Configurations.Add(new CommandPermissionMap());
            modelBuilder.Configurations.Add(new QueryPermissionMap());

            modelBuilder.Configurations.Add(new ReleaseDeploymentMeasurementMap());
            modelBuilder.Configurations.Add(new ReleaseJobMap());

            modelBuilder.Configurations.Add(new BusinessRuleDescriptionMap());
            modelBuilder.Configurations.Add(new BusinessRuleTestDataMap());
            modelBuilder.Configurations.Add(new BusinessRuleAccountTestDataMap());

            modelBuilder.Configurations.Add(new ProductRequestRegistrationMap());
            modelBuilder.Configurations.Add(new ProductRequestRegistrationTaskMap());

            modelBuilder.Configurations.Add(new ReleaseProductMap());

            base.OnModelCreating(modelBuilder);
        }
    }
}
