namespace ReMi.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitializeDatabase : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "ReMi.AccountNotification",
                c => new
                    {
                        AccountNotificationId = c.Int(nullable: false, identity: true),
                        AccountId = c.Int(nullable: false),
                        NotificationType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.AccountNotificationId)
                .ForeignKey("Auth.Accounts", t => t.AccountId, cascadeDelete: true)
                .Index(t => new { t.AccountId, t.NotificationType }, name: "IX_AccountNotification");
            
            CreateTable(
                "Auth.Accounts",
                c => new
                    {
                        AccountId = c.Int(nullable: false, identity: true),
                        ExternalId = c.Guid(nullable: false),
                        Name = c.String(nullable: false, maxLength: 128, unicode: false),
                        FullName = c.String(nullable: false, maxLength: 128, unicode: false),
                        Email = c.String(nullable: false, maxLength: 128, unicode: false),
                        RoleId = c.Int(nullable: false),
                        IsBlocked = c.Boolean(nullable: false),
                        Description = c.String(maxLength: 128, unicode: false),
                        CreatedOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.AccountId)
                .ForeignKey("Auth.Roles", t => t.RoleId)
                .Index(t => t.ExternalId)
                .Index(t => t.Email, unique: true)
                .Index(t => t.RoleId);
            
            CreateTable(
                "Auth.AccountProducts",
                c => new
                    {
                        AccountProductId = c.Int(nullable: false, identity: true),
                        AccountId = c.Int(nullable: false),
                        ProductId = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        IsDefault = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.AccountProductId)
                .ForeignKey("Auth.Accounts", t => t.AccountId, cascadeDelete: true)
                .ForeignKey("ReMi.Products", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.AccountId)
                .Index(t => t.ProductId);
            
            CreateTable(
                "ReMi.Products",
                c => new
                    {
                        ProductId = c.Int(nullable: false, identity: true),
                        Description = c.String(nullable: false, maxLength: 128, unicode: false),
                        ExternalId = c.Guid(nullable: false),
                        ReleaseTrack = c.Int(nullable: false),
                        ChooseTicketsByDefault = c.Boolean(nullable: false),
                        BusinessUnitId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ProductId)
                .ForeignKey("ReMi.BusinessUnit", t => t.BusinessUnitId, cascadeDelete: true)
                .Index(t => t.ExternalId, unique: true)
                .Index(t => t.BusinessUnitId);
            
            CreateTable(
                "ReMi.BusinessUnit",
                c => new
                    {
                        BusinessUnitId = c.Int(nullable: false, identity: true),
                        ExternalId = c.Guid(nullable: false),
                        Name = c.String(maxLength: 128),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.BusinessUnitId)
                .Index(t => t.ExternalId, unique: true)
                .Index(t => t.Name, unique: true);
            
            CreateTable(
                "ReMi.CheckListQuestionsToProducts",
                c => new
                    {
                        CheckListQuestionsToProductsId = c.Int(nullable: false, identity: true),
                        ProductId = c.Int(nullable: false),
                        CheckListQuestionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CheckListQuestionsToProductsId)
                .ForeignKey("ReMi.CheckListQuestions", t => t.CheckListQuestionId, cascadeDelete: true)
                .ForeignKey("ReMi.Products", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.ProductId)
                .Index(t => t.CheckListQuestionId);
            
            CreateTable(
                "ReMi.CheckListQuestions",
                c => new
                    {
                        CheckListQuestionId = c.Int(nullable: false, identity: true),
                        ExternalId = c.Guid(nullable: false),
                        Content = c.String(nullable: false, maxLength: 255),
                    })
                .PrimaryKey(t => t.CheckListQuestionId)
                .Index(t => t.ExternalId, unique: true);
            
            CreateTable(
                "ReMi.CheckList",
                c => new
                    {
                        CheckListId = c.Int(nullable: false, identity: true),
                        ExternalId = c.Guid(nullable: false),
                        Comment = c.String(maxLength: 4000),
                        CheckListQuestionId = c.Int(nullable: false),
                        ReleaseWindowId = c.Int(nullable: false),
                        LastChangedBy = c.String(),
                        Checked = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.CheckListId)
                .ForeignKey("ReMi.CheckListQuestions", t => t.CheckListQuestionId, cascadeDelete: true)
                .ForeignKey("ReMi.ReleaseWindows", t => t.ReleaseWindowId, cascadeDelete: true)
                .Index(t => t.CheckListQuestionId)
                .Index(t => t.ReleaseWindowId);
            
            CreateTable(
                "ReMi.ReleaseWindows",
                c => new
                    {
                        ReleaseWindowId = c.Int(nullable: false, identity: true),
                        StartTime = c.DateTime(nullable: false),
                        Sprint = c.String(nullable: false, maxLength: 128),
                        RequiresDowntime = c.Boolean(nullable: false),
                        ReleaseTypeId = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        CreatedById = c.Int(nullable: false),
                        ExternalId = c.Guid(nullable: false),
                        Description = c.String(maxLength: 1024),
                        OriginalStartTime = c.DateTime(nullable: false),
                        ReleaseDecision = c.Int(nullable: false),
                        IsFailed = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ReleaseWindowId)
                .ForeignKey("Auth.Accounts", t => t.CreatedById)
                .Index(t => t.CreatedById);
            
            CreateTable(
                "ReMi.Metrics",
                c => new
                    {
                        MetricId = c.Int(nullable: false, identity: true),
                        ExternalId = c.Guid(nullable: false),
                        ExecutedOn = c.DateTime(),
                        ReleaseWindowId = c.Int(nullable: false),
                        MetricType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MetricId)
                .ForeignKey("ReMi.ReleaseWindows", t => t.ReleaseWindowId, cascadeDelete: true)
                .Index(t => t.ExternalId, unique: true)
                .Index(t => new { t.ReleaseWindowId, t.MetricType }, unique: true);
            
            CreateTable(
                "ReMi.ReleaseApprovers",
                c => new
                    {
                        ReleaseApproverId = c.Int(nullable: false, identity: true),
                        ExternalId = c.Guid(nullable: false),
                        AccountId = c.Int(nullable: false),
                        ReleaseWindowId = c.Int(nullable: false),
                        ApprovedOn = c.DateTime(),
                        CreatedOn = c.DateTime(nullable: false),
                        Comment = c.String(),
                    })
                .PrimaryKey(t => t.ReleaseApproverId)
                .ForeignKey("Auth.Accounts", t => t.AccountId, cascadeDelete: true)
                .ForeignKey("ReMi.ReleaseWindows", t => t.ReleaseWindowId, cascadeDelete: true)
                .Index(t => t.ExternalId, unique: true)
                .Index(t => t.AccountId)
                .Index(t => t.ReleaseWindowId);
            
            CreateTable(
                "ReMi.ReleaseContent",
                c => new
                    {
                        ReleaseContentId = c.Int(nullable: false, identity: true),
                        Comment = c.String(maxLength: 1024),
                        TicketId = c.Guid(nullable: false),
                        TicketKey = c.String(nullable: false),
                        TicketRisk = c.Int(nullable: false),
                        IncludeToReleaseNotes = c.Boolean(nullable: false),
                        LastChangedByAccountId = c.Int(nullable: false),
                        ReleaseWindowsId = c.Int(),
                        Description = c.String(maxLength: 1024),
                        Assignee = c.String(maxLength: 128),
                        TicketUrl = c.String(),
                    })
                .PrimaryKey(t => t.ReleaseContentId)
                .ForeignKey("Auth.Accounts", t => t.LastChangedByAccountId, cascadeDelete: true)
                .ForeignKey("ReMi.ReleaseWindows", t => t.ReleaseWindowsId)
                .Index(t => t.TicketId, unique: true)
                .Index(t => t.LastChangedByAccountId)
                .Index(t => t.ReleaseWindowsId);
            
            CreateTable(
                "ReMi.ReleaseJobs",
                c => new
                    {
                        ReleaseJobId = c.Int(nullable: false, identity: true),
                        ExternalId = c.Guid(nullable: false),
                        Name = c.String(),
                        JobId = c.Guid(nullable: false),
                        Order = c.Int(nullable: false),
                        IsIncluded = c.Boolean(nullable: false),
                        ReleaseWindowId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ReleaseJobId)
                .ForeignKey("ReMi.ReleaseWindows", t => t.ReleaseWindowId, cascadeDelete: true)
                .Index(t => t.ExternalId, unique: true)
                .Index(t => new { t.JobId, t.ReleaseWindowId }, unique: true);
            
            CreateTable(
                "ReMi.ReleaseNotes",
                c => new
                    {
                        ReleaseNoteId = c.Int(nullable: false),
                        Issues = c.String(),
                        ReleaseNotes = c.String(),
                    })
                .PrimaryKey(t => t.ReleaseNoteId)
                .ForeignKey("ReMi.ReleaseWindows", t => t.ReleaseNoteId, cascadeDelete: true)
                .Index(t => t.ReleaseNoteId);
            
            CreateTable(
                "ReMi.ReleaseParticipant",
                c => new
                    {
                        ReleaseParticipantId = c.Int(nullable: false, identity: true),
                        AccountId = c.Int(nullable: false),
                        ReleaseWindowId = c.Int(nullable: false),
                        ExternalId = c.Guid(nullable: false),
                        ApprovedOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.ReleaseParticipantId)
                .ForeignKey("Auth.Accounts", t => t.AccountId, cascadeDelete: true)
                .ForeignKey("ReMi.ReleaseWindows", t => t.ReleaseWindowId, cascadeDelete: true)
                .Index(t => t.AccountId)
                .Index(t => t.ReleaseWindowId);
            
            CreateTable(
                "ReMi.ReleaseProducts",
                c => new
                    {
                        ReleaseProductId = c.Int(nullable: false, identity: true),
                        ProductId = c.Int(nullable: false),
                        ReleaseWindowId = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ReleaseProductId)
                .ForeignKey("ReMi.Products", t => t.ProductId, cascadeDelete: true)
                .ForeignKey("ReMi.ReleaseWindows", t => t.ReleaseWindowId, cascadeDelete: true)
                .Index(t => new { t.ProductId, t.ReleaseWindowId }, unique: true, name: "IX_ReleaseProductPair");
            
            CreateTable(
                "ReMi.ReleaseTask",
                c => new
                    {
                        ReleaseTaskId = c.Int(nullable: false, identity: true),
                        ExternalId = c.Guid(nullable: false),
                        Type = c.Int(nullable: false),
                        Description = c.String(nullable: false, maxLength: 512),
                        HelpDeskReference = c.String(),
                        HelpDeskUrl = c.String(),
                        ReleaseWindowId = c.Int(nullable: false),
                        CreatedByAccountId = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        AssigneeAccountId = c.Int(nullable: false),
                        ReceiptConfirmedOn = c.DateTime(),
                        CompletedOn = c.DateTime(),
                        RequireSiteDown = c.Boolean(nullable: false),
                        Risk = c.Int(nullable: false),
                        WhereTested = c.Int(),
                        LengthOfRun = c.Int(),
                        Order = c.Short(nullable: false),
                    })
                .PrimaryKey(t => t.ReleaseTaskId)
                .ForeignKey("Auth.Accounts", t => t.AssigneeAccountId, cascadeDelete: true)
                .ForeignKey("Auth.Accounts", t => t.CreatedByAccountId)
                .ForeignKey("ReMi.ReleaseWindows", t => t.ReleaseWindowId, cascadeDelete: true)
                .Index(t => t.ReleaseWindowId)
                .Index(t => t.CreatedByAccountId)
                .Index(t => t.AssigneeAccountId);
            
            CreateTable(
                "ReMi.ReleaseTaskAttachment",
                c => new
                    {
                        ReleaseTaskAttachmentId = c.Int(nullable: false, identity: true),
                        ExternalId = c.Guid(nullable: false),
                        Name = c.String(nullable: false, maxLength: 255),
                        Attachment = c.Binary(nullable: false),
                        HelpDeskAttachmentId = c.String(),
                        ReleaseTaskId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ReleaseTaskAttachmentId)
                .ForeignKey("ReMi.ReleaseTask", t => t.ReleaseTaskId, cascadeDelete: true)
                .Index(t => t.ReleaseTaskId);
            
            CreateTable(
                "ReMi.SignOffs",
                c => new
                    {
                        SignOffId = c.Int(nullable: false, identity: true),
                        AccountId = c.Int(nullable: false),
                        ReleaseWindowId = c.Int(nullable: false),
                        ExternalId = c.Guid(nullable: false),
                        SignedOff = c.DateTime(),
                        Comment = c.String(),
                    })
                .PrimaryKey(t => t.SignOffId)
                .ForeignKey("Auth.Accounts", t => t.AccountId, cascadeDelete: true)
                .ForeignKey("ReMi.ReleaseWindows", t => t.ReleaseWindowId, cascadeDelete: true)
                .Index(t => t.AccountId)
                .Index(t => t.ReleaseWindowId)
                .Index(t => t.ExternalId, unique: true);
            
            CreateTable(
                "ReMi.SourceControlChangesToReleaseWindows",
                c => new
                    {
                        SourceControlChangeToProductId = c.Int(nullable: false, identity: true),
                        SourceControlChangeId = c.Int(nullable: false),
                        ReleaseWindowId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.SourceControlChangeToProductId)
                .ForeignKey("ReMi.SourceControlChanges", t => t.SourceControlChangeId, cascadeDelete: true)
                .ForeignKey("ReMi.ReleaseWindows", t => t.ReleaseWindowId, cascadeDelete: true)
                .Index(t => t.SourceControlChangeId)
                .Index(t => t.ReleaseWindowId);
            
            CreateTable(
                "ReMi.SourceControlChanges",
                c => new
                    {
                        SourceControlChangeId = c.Int(nullable: false, identity: true),
                        Owner = c.String(nullable: false, maxLength: 256),
                        Description = c.String(nullable: false, maxLength: 2048),
                        Identifier = c.String(nullable: false, maxLength: 256),
                        Repository = c.String(nullable: false, maxLength: 256),
                        Date = c.DateTime(),
                    })
                .PrimaryKey(t => t.SourceControlChangeId)
                .Index(t => t.Identifier, unique: true);
            
            CreateTable(
                "Plugin.PluginPackageConfiguration",
                c => new
                    {
                        PluginPackageConfigurationId = c.Int(nullable: false, identity: true),
                        ExternalId = c.Guid(nullable: false),
                        PluginType = c.Int(nullable: false),
                        PackageId = c.Int(nullable: false),
                        PluginId = c.Int(),
                    })
                .PrimaryKey(t => t.PluginPackageConfigurationId)
                .ForeignKey("ReMi.Products", t => t.PackageId, cascadeDelete: true)
                .ForeignKey("Plugin.Plugins", t => t.PluginId)
                .Index(t => t.ExternalId, unique: true)
                .Index(t => new { t.PluginType, t.PackageId }, unique: true, name: "IX_PluginType_Package")
                .Index(t => t.PluginId);
            
            CreateTable(
                "Plugin.Plugins",
                c => new
                    {
                        PluginId = c.Int(nullable: false, identity: true),
                        ExternalId = c.Guid(nullable: false),
                        Key = c.String(maxLength: 256),
                        PluginType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PluginId)
                .Index(t => t.ExternalId, unique: true)
                .Index(t => t.Key, unique: true);
            
            CreateTable(
                "Auth.Roles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ExternalId = c.Guid(nullable: false),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.ExternalId, unique: true)
                .Index(t => t.Name, unique: true);
            
            CreateTable(
                "Auth.Sessions",
                c => new
                    {
                        SessionId = c.Int(nullable: false, identity: true),
                        ExternalId = c.Guid(nullable: false),
                        AccountId = c.Int(nullable: false),
                        ExpireAfter = c.DateTime(),
                        Completed = c.DateTime(),
                        Description = c.String(maxLength: 128, unicode: false),
                        CreatedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.SessionId)
                .ForeignKey("Auth.Accounts", t => t.AccountId, cascadeDelete: true)
                .Index(t => t.AccountId);
            
            CreateTable(
                "BR.BusinessRule",
                c => new
                    {
                        BusinessRuleId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Group = c.Int(nullable: false),
                        Description = c.String(nullable: false, maxLength: 256),
                        ExternalId = c.Guid(nullable: false),
                        Script = c.String(),
                    })
                .PrimaryKey(t => t.BusinessRuleId)
                .Index(t => new { t.Name, t.Group }, unique: true, name: "IX_NameGroupUnique")
                .Index(t => t.ExternalId, unique: true);
            
            CreateTable(
                "BR.BusinessRuleAccountTestData",
                c => new
                    {
                        BusinessRuleAccountTestDataId = c.Int(nullable: false),
                        ExternalId = c.Guid(nullable: false),
                        JsonData = c.String(),
                    })
                .PrimaryKey(t => t.BusinessRuleAccountTestDataId)
                .ForeignKey("BR.BusinessRule", t => t.BusinessRuleAccountTestDataId, cascadeDelete: true)
                .Index(t => t.BusinessRuleAccountTestDataId)
                .Index(t => t.ExternalId, unique: true);
            
            CreateTable(
                "BR.BusinessRuleParameter",
                c => new
                    {
                        BusinessRuleParameterId = c.Int(nullable: false, identity: true),
                        ExternalId = c.Guid(nullable: false),
                        BusinessRuleId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 50),
                        Type = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.BusinessRuleParameterId)
                .ForeignKey("BR.BusinessRule", t => t.BusinessRuleId, cascadeDelete: true)
                .Index(t => t.ExternalId, unique: true)
                .Index(t => new { t.BusinessRuleId, t.Name }, name: "IX_BusinessRuleAndName");
            
            CreateTable(
                "BR.BusinessRuleTestData",
                c => new
                    {
                        BusinessRuleTestDataId = c.Int(nullable: false),
                        ExternalId = c.Guid(nullable: false),
                        JsonData = c.String(),
                    })
                .PrimaryKey(t => t.BusinessRuleTestDataId)
                .ForeignKey("BR.BusinessRuleParameter", t => t.BusinessRuleTestDataId, cascadeDelete: true)
                .Index(t => t.BusinessRuleTestDataId)
                .Index(t => t.ExternalId, unique: true);
            
            CreateTable(
                "BR.BusinessRuleGroup",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true);
            
            CreateTable(
                "ExecPoll.CommandExecutions",
                c => new
                    {
                        CommandExecutionId = c.Int(nullable: false, identity: true),
                        ExternalId = c.Guid(nullable: false),
                        Description = c.String(nullable: false, maxLength: 128, unicode: false),
                        CreatedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.CommandExecutionId);
            
            CreateTable(
                "ExecPoll.CommandHistory",
                c => new
                    {
                        CommandHistoryId = c.Int(nullable: false, identity: true),
                        CommandExecutionId = c.Int(nullable: false),
                        State = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        Details = c.String(),
                    })
                .PrimaryKey(t => t.CommandHistoryId)
                .ForeignKey("ExecPoll.CommandExecutions", t => t.CommandExecutionId, cascadeDelete: true)
                .Index(t => t.CommandExecutionId);
            
            CreateTable(
                "Auth.CommandPermissions",
                c => new
                    {
                        CommandPermissionId = c.Int(nullable: false, identity: true),
                        RoleId = c.Int(nullable: false),
                        CommandId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CommandPermissionId)
                .ForeignKey("Api.Commands", t => t.CommandId, cascadeDelete: true)
                .ForeignKey("Auth.Roles", t => t.RoleId)
                .Index(t => t.RoleId)
                .Index(t => t.CommandId);
            
            CreateTable(
                "Api.Commands",
                c => new
                    {
                        CommandId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 256),
                        Group = c.String(nullable: false, maxLength: 256),
                        Description = c.String(nullable: false, maxLength: 256),
                        Namespace = c.String(maxLength: 256),
                        IsBackground = c.Boolean(nullable: false),
                        RuleId = c.Int(),
                    })
                .PrimaryKey(t => t.CommandId)
                .ForeignKey("BR.BusinessRule", t => t.RuleId)
                .Index(t => t.Name, unique: true)
                .Index(t => t.RuleId);
            
            CreateTable(
                "ExecPoll.CommandStateTypes",
                c => new
                    {
                        CommandStateTypeId = c.Int(nullable: false, identity: true),
                        Description = c.String(nullable: false, maxLength: 128, unicode: false),
                    })
                .PrimaryKey(t => t.CommandStateTypeId);
            
            CreateTable(
                "Api.Descriptions",
                c => new
                    {
                        DescriptionId = c.Int(nullable: false, identity: true),
                        DescriptionText = c.String(),
                        Url = c.String(nullable: false, maxLength: 512),
                        HttpMethod = c.String(nullable: false, maxLength: 16),
                    })
                .PrimaryKey(t => t.DescriptionId);
            
            CreateTable(
                "Evt.EventHistory",
                c => new
                    {
                        EventHistoryId = c.Int(nullable: false, identity: true),
                        EventId = c.Int(nullable: false),
                        State = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        Details = c.String(),
                    })
                .PrimaryKey(t => t.EventHistoryId)
                .ForeignKey("Evt.Events", t => t.EventId, cascadeDelete: true)
                .Index(t => t.EventId);
            
            CreateTable(
                "Evt.Events",
                c => new
                    {
                        EventId = c.Int(nullable: false, identity: true),
                        ExternalId = c.Guid(nullable: false),
                        Data = c.String(maxLength: 4096, unicode: false),
                        Description = c.String(nullable: false, maxLength: 128, unicode: false),
                        Handler = c.String(maxLength: 1024, unicode: false),
                        CreatedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.EventId);
            
            CreateTable(
                "Evt.EventStateTypes",
                c => new
                    {
                        EventStateTypeId = c.Int(nullable: false, identity: true),
                        Description = c.String(nullable: false, maxLength: 128, unicode: false),
                    })
                .PrimaryKey(t => t.EventStateTypeId);
            
            CreateTable(
                "ReMi.MetricTypes",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        IsBackground = c.Boolean(),
                        Order = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true);
            
            CreateTable(
                "ReMi.NotificationType",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true);
            
            CreateTable(
                "Plugin.PluginConfiguration",
                c => new
                    {
                        PluginConfigurationId = c.Int(nullable: false, identity: true),
                        ExternalId = c.Guid(nullable: false),
                        PluginType = c.Int(nullable: false),
                        PluginId = c.Int(),
                    })
                .PrimaryKey(t => t.PluginConfigurationId)
                .ForeignKey("Plugin.Plugins", t => t.PluginId)
                .Index(t => t.ExternalId, unique: true)
                .Index(t => t.PluginType, unique: true)
                .Index(t => t.PluginId);
            
            CreateTable(
                "Plugin.PluginTypes",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        IsGlobal = c.Boolean(),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true);
            
            CreateTable(
                "ReMi.ProductRequestGroups",
                c => new
                    {
                        ProductRequestGroupId = c.Int(nullable: false, identity: true),
                        ProductRequestTypeId = c.Int(nullable: false),
                        ExternalId = c.Guid(nullable: false),
                        Name = c.String(nullable: false, maxLength: 1024),
                    })
                .PrimaryKey(t => t.ProductRequestGroupId)
                .ForeignKey("ReMi.ProductRequestTypes", t => t.ProductRequestTypeId, cascadeDelete: true)
                .Index(t => t.ProductRequestTypeId)
                .Index(t => t.ExternalId, unique: true);
            
            CreateTable(
                "ReMi.ProductRequestGroupAssignees",
                c => new
                    {
                        ProductRequestGroupAssigneeId = c.Int(nullable: false, identity: true),
                        ProductRequestGroupId = c.Int(nullable: false),
                        AccountId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ProductRequestGroupAssigneeId)
                .ForeignKey("Auth.Accounts", t => t.AccountId, cascadeDelete: true)
                .ForeignKey("ReMi.ProductRequestGroups", t => t.ProductRequestGroupId, cascadeDelete: true)
                .Index(t => t.ProductRequestGroupId)
                .Index(t => t.AccountId);
            
            CreateTable(
                "ReMi.ProductRequestTasks",
                c => new
                    {
                        ProductRequestTaskId = c.Int(nullable: false, identity: true),
                        ProductRequestGroupId = c.Int(nullable: false),
                        ExternalId = c.Guid(nullable: false),
                        Question = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.ProductRequestTaskId)
                .ForeignKey("ReMi.ProductRequestGroups", t => t.ProductRequestGroupId, cascadeDelete: true)
                .Index(t => t.ProductRequestGroupId)
                .Index(t => t.ExternalId, unique: true);
            
            CreateTable(
                "ReMi.ProductRequestRegistrationTasks",
                c => new
                    {
                        ProductRequestRegistrationTaskId = c.Int(nullable: false, identity: true),
                        ProductRequestRegistrationId = c.Int(nullable: false),
                        ProductRequestTaskId = c.Int(nullable: false),
                        IsCompleted = c.Boolean(nullable: false),
                        Comment = c.String(),
                        LastChangedByAccountId = c.Int(),
                        LastChangedOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.ProductRequestRegistrationTaskId)
                .ForeignKey("Auth.Accounts", t => t.LastChangedByAccountId)
                .ForeignKey("ReMi.ProductRequestRegistrations", t => t.ProductRequestRegistrationId, cascadeDelete: true)
                .ForeignKey("ReMi.ProductRequestTasks", t => t.ProductRequestTaskId)
                .Index(t => t.ProductRequestRegistrationId)
                .Index(t => t.ProductRequestTaskId)
                .Index(t => t.LastChangedByAccountId);
            
            CreateTable(
                "ReMi.ProductRequestRegistrations",
                c => new
                    {
                        ProductRequestRegistrationId = c.Int(nullable: false, identity: true),
                        ExternalId = c.Guid(nullable: false),
                        ProductRequestTypeId = c.Int(nullable: false),
                        Description = c.String(nullable: false, maxLength: 1024),
                        CreatedOn = c.DateTime(nullable: false),
                        CreatedByAccountId = c.Int(nullable: false),
                        Deleted = c.Boolean(),
                    })
                .PrimaryKey(t => t.ProductRequestRegistrationId)
                .ForeignKey("Auth.Accounts", t => t.CreatedByAccountId)
                .ForeignKey("ReMi.ProductRequestTypes", t => t.ProductRequestTypeId)
                .Index(t => t.ExternalId, unique: true)
                .Index(t => t.ProductRequestTypeId)
                .Index(t => t.CreatedByAccountId);
            
            CreateTable(
                "ReMi.ProductRequestTypes",
                c => new
                    {
                        ProductRequestTypeId = c.Int(nullable: false, identity: true),
                        ExternalId = c.Guid(nullable: false),
                        Name = c.String(nullable: false, maxLength: 1024),
                    })
                .PrimaryKey(t => t.ProductRequestTypeId)
                .Index(t => t.ExternalId, unique: true);
            
            CreateTable(
                "ReMi.ProductRequestRegistrationRemovingReasons",
                c => new
                    {
                        ProductRequestRegistrationId = c.Int(nullable: false),
                        RemovingReason = c.Int(nullable: false),
                        Comment = c.String(),
                    })
                .PrimaryKey(t => t.ProductRequestRegistrationId)
                .ForeignKey("ReMi.ProductRequestRegistrations", t => t.ProductRequestRegistrationId, cascadeDelete: true)
                .Index(t => t.ProductRequestRegistrationId);
            
            CreateTable(
                "Api.Queries",
                c => new
                    {
                        QueryId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 256),
                        Group = c.String(nullable: false, maxLength: 256),
                        Description = c.String(nullable: false, maxLength: 256),
                        Namespace = c.String(nullable: false, maxLength: 256),
                        IsStatic = c.Boolean(nullable: false),
                        RuleId = c.Int(),
                    })
                .PrimaryKey(t => t.QueryId)
                .ForeignKey("BR.BusinessRule", t => t.RuleId)
                .Index(t => t.Name, unique: true)
                .Index(t => t.RuleId);
            
            CreateTable(
                "Auth.QueryPermissions",
                c => new
                    {
                        QueryPermissionId = c.Int(nullable: false, identity: true),
                        RoleId = c.Int(nullable: false),
                        QueryId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.QueryPermissionId)
                .ForeignKey("Api.Queries", t => t.QueryId, cascadeDelete: true)
                .ForeignKey("Auth.Roles", t => t.RoleId)
                .Index(t => t.RoleId)
                .Index(t => t.QueryId);
            
            CreateTable(
                "ReMi.ReleaseDecision",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true);
            
            CreateTable(
                "ReMi.ReleaseDeploymentMeasurements",
                c => new
                    {
                        ReleaseDeploymentMeasurementId = c.Int(nullable: false, identity: true),
                        ReleaseWindowId = c.Int(nullable: false),
                        ParentMeasurementId = c.Int(),
                        StepName = c.String(maxLength: 256),
                        Locator = c.String(),
                        StepId = c.String(),
                        StartTime = c.DateTime(),
                        FinishTime = c.DateTime(),
                        CreatedOn = c.DateTime(nullable: false),
                        CreatedByAccountId = c.Int(nullable: false),
                        BuildNumber = c.Int(),
                        NumberOfTries = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ReleaseDeploymentMeasurementId)
                .ForeignKey("Auth.Accounts", t => t.CreatedByAccountId)
                .ForeignKey("ReMi.ReleaseDeploymentMeasurements", t => t.ParentMeasurementId)
                .ForeignKey("ReMi.ReleaseWindows", t => t.ReleaseWindowId, cascadeDelete: true)
                .Index(t => t.ReleaseWindowId)
                .Index(t => t.ParentMeasurementId)
                .Index(t => t.StepName)
                .Index(t => t.CreatedByAccountId)
                .Index(t => t.BuildNumber);
            
            CreateTable(
                "ReMi.ReleaseRepositories",
                c => new
                    {
                        ReleaseRepositoryId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        RepositoryId = c.Guid(nullable: false),
                        LatestChange = c.Boolean(nullable: false),
                        ChangesFrom = c.String(),
                        ChangesTo = c.String(),
                        IsIncluded = c.Boolean(nullable: false),
                        ReleaseWindowId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ReleaseRepositoryId)
                .ForeignKey("ReMi.ReleaseWindows", t => t.ReleaseWindowId, cascadeDelete: true)
                .Index(t => new { t.RepositoryId, t.ReleaseWindowId }, unique: true);
            
            CreateTable(
                "ReMi.ReleaseTaskEnvironments",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true);
            
            CreateTable(
                "ReMi.ReleaseTaskRisks",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true);
            
            CreateTable(
                "ReMi.ReleaseTaskType",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true);
            
            CreateTable(
                "ReMi.ReleaseTrack",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true);
            
            CreateTable(
                "ReMi.ReleaseTypes",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        IsMaintenance = c.Boolean(),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true);
            
            CreateTable(
                "ReMi.RemovingReasons",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true);
            
            CreateTable(
                "Report.ReportColumns",
                c => new
                    {
                        ReportColumnId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 256),
                        ReportDescriptionId = c.Int(nullable: false),
                        Order = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ReportColumnId)
                .ForeignKey("Report.ReportDescriptions", t => t.ReportDescriptionId, cascadeDelete: true)
                .Index(t => new { t.ReportDescriptionId, t.Order, t.Name }, unique: true, name: "IX_ReportColumnOrder");
            
            CreateTable(
                "Report.ReportDescriptions",
                c => new
                    {
                        ReportDescriptionId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 256),
                        ProcedureName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.ReportDescriptionId)
                .Index(t => t.Name, unique: true);
            
            CreateTable(
                "Report.ReportParameters",
                c => new
                    {
                        ReportParameterId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 256),
                        ReportDescriptionId = c.Int(nullable: false),
                        Description = c.String(nullable: false, maxLength: 256),
                        Type = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.ReportParameterId)
                .ForeignKey("Report.ReportDescriptions", t => t.ReportDescriptionId, cascadeDelete: true)
                .Index(t => new { t.ReportDescriptionId, t.Name }, unique: true, name: "IX_ReportParameter");
            
            CreateTable(
                "ReMi.TicketRisk",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true);
            
        }
        
        public override void Down()
        {
            DropForeignKey("Report.ReportParameters", "ReportDescriptionId", "Report.ReportDescriptions");
            DropForeignKey("Report.ReportColumns", "ReportDescriptionId", "Report.ReportDescriptions");
            DropForeignKey("ReMi.ReleaseRepositories", "ReleaseWindowId", "ReMi.ReleaseWindows");
            DropForeignKey("ReMi.ReleaseDeploymentMeasurements", "ReleaseWindowId", "ReMi.ReleaseWindows");
            DropForeignKey("ReMi.ReleaseDeploymentMeasurements", "ParentMeasurementId", "ReMi.ReleaseDeploymentMeasurements");
            DropForeignKey("ReMi.ReleaseDeploymentMeasurements", "CreatedByAccountId", "Auth.Accounts");
            DropForeignKey("Api.Queries", "RuleId", "BR.BusinessRule");
            DropForeignKey("Auth.QueryPermissions", "RoleId", "Auth.Roles");
            DropForeignKey("Auth.QueryPermissions", "QueryId", "Api.Queries");
            DropForeignKey("ReMi.ProductRequestTasks", "ProductRequestGroupId", "ReMi.ProductRequestGroups");
            DropForeignKey("ReMi.ProductRequestRegistrationTasks", "ProductRequestTaskId", "ReMi.ProductRequestTasks");
            DropForeignKey("ReMi.ProductRequestRegistrationTasks", "ProductRequestRegistrationId", "ReMi.ProductRequestRegistrations");
            DropForeignKey("ReMi.ProductRequestRegistrationRemovingReasons", "ProductRequestRegistrationId", "ReMi.ProductRequestRegistrations");
            DropForeignKey("ReMi.ProductRequestRegistrations", "ProductRequestTypeId", "ReMi.ProductRequestTypes");
            DropForeignKey("ReMi.ProductRequestGroups", "ProductRequestTypeId", "ReMi.ProductRequestTypes");
            DropForeignKey("ReMi.ProductRequestRegistrations", "CreatedByAccountId", "Auth.Accounts");
            DropForeignKey("ReMi.ProductRequestRegistrationTasks", "LastChangedByAccountId", "Auth.Accounts");
            DropForeignKey("ReMi.ProductRequestGroupAssignees", "ProductRequestGroupId", "ReMi.ProductRequestGroups");
            DropForeignKey("ReMi.ProductRequestGroupAssignees", "AccountId", "Auth.Accounts");
            DropForeignKey("Plugin.PluginConfiguration", "PluginId", "Plugin.Plugins");
            DropForeignKey("Evt.EventHistory", "EventId", "Evt.Events");
            DropForeignKey("Auth.CommandPermissions", "RoleId", "Auth.Roles");
            DropForeignKey("Auth.CommandPermissions", "CommandId", "Api.Commands");
            DropForeignKey("Api.Commands", "RuleId", "BR.BusinessRule");
            DropForeignKey("ExecPoll.CommandHistory", "CommandExecutionId", "ExecPoll.CommandExecutions");
            DropForeignKey("BR.BusinessRuleParameter", "BusinessRuleId", "BR.BusinessRule");
            DropForeignKey("BR.BusinessRuleTestData", "BusinessRuleTestDataId", "BR.BusinessRuleParameter");
            DropForeignKey("BR.BusinessRuleAccountTestData", "BusinessRuleAccountTestDataId", "BR.BusinessRule");
            DropForeignKey("Auth.Sessions", "AccountId", "Auth.Accounts");
            DropForeignKey("Auth.Accounts", "RoleId", "Auth.Roles");
            DropForeignKey("Plugin.PluginPackageConfiguration", "PluginId", "Plugin.Plugins");
            DropForeignKey("Plugin.PluginPackageConfiguration", "PackageId", "ReMi.Products");
            DropForeignKey("ReMi.CheckListQuestionsToProducts", "ProductId", "ReMi.Products");
            DropForeignKey("ReMi.SourceControlChangesToReleaseWindows", "ReleaseWindowId", "ReMi.ReleaseWindows");
            DropForeignKey("ReMi.SourceControlChangesToReleaseWindows", "SourceControlChangeId", "ReMi.SourceControlChanges");
            DropForeignKey("ReMi.SignOffs", "ReleaseWindowId", "ReMi.ReleaseWindows");
            DropForeignKey("ReMi.SignOffs", "AccountId", "Auth.Accounts");
            DropForeignKey("ReMi.ReleaseTask", "ReleaseWindowId", "ReMi.ReleaseWindows");
            DropForeignKey("ReMi.ReleaseTask", "CreatedByAccountId", "Auth.Accounts");
            DropForeignKey("ReMi.ReleaseTaskAttachment", "ReleaseTaskId", "ReMi.ReleaseTask");
            DropForeignKey("ReMi.ReleaseTask", "AssigneeAccountId", "Auth.Accounts");
            DropForeignKey("ReMi.ReleaseProducts", "ReleaseWindowId", "ReMi.ReleaseWindows");
            DropForeignKey("ReMi.ReleaseProducts", "ProductId", "ReMi.Products");
            DropForeignKey("ReMi.ReleaseParticipant", "ReleaseWindowId", "ReMi.ReleaseWindows");
            DropForeignKey("ReMi.ReleaseParticipant", "AccountId", "Auth.Accounts");
            DropForeignKey("ReMi.ReleaseNotes", "ReleaseNoteId", "ReMi.ReleaseWindows");
            DropForeignKey("ReMi.ReleaseJobs", "ReleaseWindowId", "ReMi.ReleaseWindows");
            DropForeignKey("ReMi.ReleaseContent", "ReleaseWindowsId", "ReMi.ReleaseWindows");
            DropForeignKey("ReMi.ReleaseContent", "LastChangedByAccountId", "Auth.Accounts");
            DropForeignKey("ReMi.ReleaseApprovers", "ReleaseWindowId", "ReMi.ReleaseWindows");
            DropForeignKey("ReMi.ReleaseApprovers", "AccountId", "Auth.Accounts");
            DropForeignKey("ReMi.Metrics", "ReleaseWindowId", "ReMi.ReleaseWindows");
            DropForeignKey("ReMi.ReleaseWindows", "CreatedById", "Auth.Accounts");
            DropForeignKey("ReMi.CheckList", "ReleaseWindowId", "ReMi.ReleaseWindows");
            DropForeignKey("ReMi.CheckList", "CheckListQuestionId", "ReMi.CheckListQuestions");
            DropForeignKey("ReMi.CheckListQuestionsToProducts", "CheckListQuestionId", "ReMi.CheckListQuestions");
            DropForeignKey("ReMi.Products", "BusinessUnitId", "ReMi.BusinessUnit");
            DropForeignKey("Auth.AccountProducts", "ProductId", "ReMi.Products");
            DropForeignKey("Auth.AccountProducts", "AccountId", "Auth.Accounts");
            DropForeignKey("ReMi.AccountNotification", "AccountId", "Auth.Accounts");
            DropIndex("ReMi.TicketRisk", new[] { "Name" });
            DropIndex("Report.ReportParameters", "IX_ReportParameter");
            DropIndex("Report.ReportDescriptions", new[] { "Name" });
            DropIndex("Report.ReportColumns", "IX_ReportColumnOrder");
            DropIndex("ReMi.RemovingReasons", new[] { "Name" });
            DropIndex("ReMi.ReleaseTypes", new[] { "Name" });
            DropIndex("ReMi.ReleaseTrack", new[] { "Name" });
            DropIndex("ReMi.ReleaseTaskType", new[] { "Name" });
            DropIndex("ReMi.ReleaseTaskRisks", new[] { "Name" });
            DropIndex("ReMi.ReleaseTaskEnvironments", new[] { "Name" });
            DropIndex("ReMi.ReleaseRepositories", new[] { "RepositoryId", "ReleaseWindowId" });
            DropIndex("ReMi.ReleaseDeploymentMeasurements", new[] { "BuildNumber" });
            DropIndex("ReMi.ReleaseDeploymentMeasurements", new[] { "CreatedByAccountId" });
            DropIndex("ReMi.ReleaseDeploymentMeasurements", new[] { "StepName" });
            DropIndex("ReMi.ReleaseDeploymentMeasurements", new[] { "ParentMeasurementId" });
            DropIndex("ReMi.ReleaseDeploymentMeasurements", new[] { "ReleaseWindowId" });
            DropIndex("ReMi.ReleaseDecision", new[] { "Name" });
            DropIndex("Auth.QueryPermissions", new[] { "QueryId" });
            DropIndex("Auth.QueryPermissions", new[] { "RoleId" });
            DropIndex("Api.Queries", new[] { "RuleId" });
            DropIndex("Api.Queries", new[] { "Name" });
            DropIndex("ReMi.ProductRequestRegistrationRemovingReasons", new[] { "ProductRequestRegistrationId" });
            DropIndex("ReMi.ProductRequestTypes", new[] { "ExternalId" });
            DropIndex("ReMi.ProductRequestRegistrations", new[] { "CreatedByAccountId" });
            DropIndex("ReMi.ProductRequestRegistrations", new[] { "ProductRequestTypeId" });
            DropIndex("ReMi.ProductRequestRegistrations", new[] { "ExternalId" });
            DropIndex("ReMi.ProductRequestRegistrationTasks", new[] { "LastChangedByAccountId" });
            DropIndex("ReMi.ProductRequestRegistrationTasks", new[] { "ProductRequestTaskId" });
            DropIndex("ReMi.ProductRequestRegistrationTasks", new[] { "ProductRequestRegistrationId" });
            DropIndex("ReMi.ProductRequestTasks", new[] { "ExternalId" });
            DropIndex("ReMi.ProductRequestTasks", new[] { "ProductRequestGroupId" });
            DropIndex("ReMi.ProductRequestGroupAssignees", new[] { "AccountId" });
            DropIndex("ReMi.ProductRequestGroupAssignees", new[] { "ProductRequestGroupId" });
            DropIndex("ReMi.ProductRequestGroups", new[] { "ExternalId" });
            DropIndex("ReMi.ProductRequestGroups", new[] { "ProductRequestTypeId" });
            DropIndex("Plugin.PluginTypes", new[] { "Name" });
            DropIndex("Plugin.PluginConfiguration", new[] { "PluginId" });
            DropIndex("Plugin.PluginConfiguration", new[] { "PluginType" });
            DropIndex("Plugin.PluginConfiguration", new[] { "ExternalId" });
            DropIndex("ReMi.NotificationType", new[] { "Name" });
            DropIndex("ReMi.MetricTypes", new[] { "Name" });
            DropIndex("Evt.EventHistory", new[] { "EventId" });
            DropIndex("Api.Commands", new[] { "RuleId" });
            DropIndex("Api.Commands", new[] { "Name" });
            DropIndex("Auth.CommandPermissions", new[] { "CommandId" });
            DropIndex("Auth.CommandPermissions", new[] { "RoleId" });
            DropIndex("ExecPoll.CommandHistory", new[] { "CommandExecutionId" });
            DropIndex("BR.BusinessRuleGroup", new[] { "Name" });
            DropIndex("BR.BusinessRuleTestData", new[] { "ExternalId" });
            DropIndex("BR.BusinessRuleTestData", new[] { "BusinessRuleTestDataId" });
            DropIndex("BR.BusinessRuleParameter", "IX_BusinessRuleAndName");
            DropIndex("BR.BusinessRuleParameter", new[] { "ExternalId" });
            DropIndex("BR.BusinessRuleAccountTestData", new[] { "ExternalId" });
            DropIndex("BR.BusinessRuleAccountTestData", new[] { "BusinessRuleAccountTestDataId" });
            DropIndex("BR.BusinessRule", new[] { "ExternalId" });
            DropIndex("BR.BusinessRule", "IX_NameGroupUnique");
            DropIndex("Auth.Sessions", new[] { "AccountId" });
            DropIndex("Auth.Roles", new[] { "Name" });
            DropIndex("Auth.Roles", new[] { "ExternalId" });
            DropIndex("Plugin.Plugins", new[] { "Key" });
            DropIndex("Plugin.Plugins", new[] { "ExternalId" });
            DropIndex("Plugin.PluginPackageConfiguration", new[] { "PluginId" });
            DropIndex("Plugin.PluginPackageConfiguration", "IX_PluginType_Package");
            DropIndex("Plugin.PluginPackageConfiguration", new[] { "ExternalId" });
            DropIndex("ReMi.SourceControlChanges", new[] { "Identifier" });
            DropIndex("ReMi.SourceControlChangesToReleaseWindows", new[] { "ReleaseWindowId" });
            DropIndex("ReMi.SourceControlChangesToReleaseWindows", new[] { "SourceControlChangeId" });
            DropIndex("ReMi.SignOffs", new[] { "ExternalId" });
            DropIndex("ReMi.SignOffs", new[] { "ReleaseWindowId" });
            DropIndex("ReMi.SignOffs", new[] { "AccountId" });
            DropIndex("ReMi.ReleaseTaskAttachment", new[] { "ReleaseTaskId" });
            DropIndex("ReMi.ReleaseTask", new[] { "AssigneeAccountId" });
            DropIndex("ReMi.ReleaseTask", new[] { "CreatedByAccountId" });
            DropIndex("ReMi.ReleaseTask", new[] { "ReleaseWindowId" });
            DropIndex("ReMi.ReleaseProducts", "IX_ReleaseProductPair");
            DropIndex("ReMi.ReleaseParticipant", new[] { "ReleaseWindowId" });
            DropIndex("ReMi.ReleaseParticipant", new[] { "AccountId" });
            DropIndex("ReMi.ReleaseNotes", new[] { "ReleaseNoteId" });
            DropIndex("ReMi.ReleaseJobs", new[] { "JobId", "ReleaseWindowId" });
            DropIndex("ReMi.ReleaseJobs", new[] { "ExternalId" });
            DropIndex("ReMi.ReleaseContent", new[] { "ReleaseWindowsId" });
            DropIndex("ReMi.ReleaseContent", new[] { "LastChangedByAccountId" });
            DropIndex("ReMi.ReleaseContent", new[] { "TicketId" });
            DropIndex("ReMi.ReleaseApprovers", new[] { "ReleaseWindowId" });
            DropIndex("ReMi.ReleaseApprovers", new[] { "AccountId" });
            DropIndex("ReMi.ReleaseApprovers", new[] { "ExternalId" });
            DropIndex("ReMi.Metrics", new[] { "ReleaseWindowId", "MetricType" });
            DropIndex("ReMi.Metrics", new[] { "ExternalId" });
            DropIndex("ReMi.ReleaseWindows", new[] { "CreatedById" });
            DropIndex("ReMi.CheckList", new[] { "ReleaseWindowId" });
            DropIndex("ReMi.CheckList", new[] { "CheckListQuestionId" });
            DropIndex("ReMi.CheckListQuestions", new[] { "ExternalId" });
            DropIndex("ReMi.CheckListQuestionsToProducts", new[] { "CheckListQuestionId" });
            DropIndex("ReMi.CheckListQuestionsToProducts", new[] { "ProductId" });
            DropIndex("ReMi.BusinessUnit", new[] { "Name" });
            DropIndex("ReMi.BusinessUnit", new[] { "ExternalId" });
            DropIndex("ReMi.Products", new[] { "BusinessUnitId" });
            DropIndex("ReMi.Products", new[] { "ExternalId" });
            DropIndex("Auth.AccountProducts", new[] { "ProductId" });
            DropIndex("Auth.AccountProducts", new[] { "AccountId" });
            DropIndex("Auth.Accounts", new[] { "RoleId" });
            DropIndex("Auth.Accounts", new[] { "Email" });
            DropIndex("Auth.Accounts", new[] { "ExternalId" });
            DropIndex("ReMi.AccountNotification", "IX_AccountNotification");
            DropTable("ReMi.TicketRisk");
            DropTable("Report.ReportParameters");
            DropTable("Report.ReportDescriptions");
            DropTable("Report.ReportColumns");
            DropTable("ReMi.RemovingReasons");
            DropTable("ReMi.ReleaseTypes");
            DropTable("ReMi.ReleaseTrack");
            DropTable("ReMi.ReleaseTaskType");
            DropTable("ReMi.ReleaseTaskRisks");
            DropTable("ReMi.ReleaseTaskEnvironments");
            DropTable("ReMi.ReleaseRepositories");
            DropTable("ReMi.ReleaseDeploymentMeasurements");
            DropTable("ReMi.ReleaseDecision");
            DropTable("Auth.QueryPermissions");
            DropTable("Api.Queries");
            DropTable("ReMi.ProductRequestRegistrationRemovingReasons");
            DropTable("ReMi.ProductRequestTypes");
            DropTable("ReMi.ProductRequestRegistrations");
            DropTable("ReMi.ProductRequestRegistrationTasks");
            DropTable("ReMi.ProductRequestTasks");
            DropTable("ReMi.ProductRequestGroupAssignees");
            DropTable("ReMi.ProductRequestGroups");
            DropTable("Plugin.PluginTypes");
            DropTable("Plugin.PluginConfiguration");
            DropTable("ReMi.NotificationType");
            DropTable("ReMi.MetricTypes");
            DropTable("Evt.EventStateTypes");
            DropTable("Evt.Events");
            DropTable("Evt.EventHistory");
            DropTable("Api.Descriptions");
            DropTable("ExecPoll.CommandStateTypes");
            DropTable("Api.Commands");
            DropTable("Auth.CommandPermissions");
            DropTable("ExecPoll.CommandHistory");
            DropTable("ExecPoll.CommandExecutions");
            DropTable("BR.BusinessRuleGroup");
            DropTable("BR.BusinessRuleTestData");
            DropTable("BR.BusinessRuleParameter");
            DropTable("BR.BusinessRuleAccountTestData");
            DropTable("BR.BusinessRule");
            DropTable("Auth.Sessions");
            DropTable("Auth.Roles");
            DropTable("Plugin.Plugins");
            DropTable("Plugin.PluginPackageConfiguration");
            DropTable("ReMi.SourceControlChanges");
            DropTable("ReMi.SourceControlChangesToReleaseWindows");
            DropTable("ReMi.SignOffs");
            DropTable("ReMi.ReleaseTaskAttachment");
            DropTable("ReMi.ReleaseTask");
            DropTable("ReMi.ReleaseProducts");
            DropTable("ReMi.ReleaseParticipant");
            DropTable("ReMi.ReleaseNotes");
            DropTable("ReMi.ReleaseJobs");
            DropTable("ReMi.ReleaseContent");
            DropTable("ReMi.ReleaseApprovers");
            DropTable("ReMi.Metrics");
            DropTable("ReMi.ReleaseWindows");
            DropTable("ReMi.CheckList");
            DropTable("ReMi.CheckListQuestions");
            DropTable("ReMi.CheckListQuestionsToProducts");
            DropTable("ReMi.BusinessUnit");
            DropTable("ReMi.Products");
            DropTable("Auth.AccountProducts");
            DropTable("Auth.Accounts");
            DropTable("ReMi.AccountNotification");
        }
    }
}
