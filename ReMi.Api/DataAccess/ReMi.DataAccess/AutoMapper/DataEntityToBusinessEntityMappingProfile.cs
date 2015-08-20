using AutoMapper;
using ReMi.BusinessEntities.ReleaseExecution;
using ReMi.Common.Constants.ReleaseExecution;
using ReMi.Common.Constants.ReleasePlan;
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
using System;
using System.Linq;
using ReMi.BusinessEntities.DeploymentTool;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Utils.Enums;
using ReMi.DataEntities.Plugins;
using ReleaseJob = ReMi.DataEntities.ReleasePlan.ReleaseJob;

namespace ReMi.DataAccess.AutoMapper
{
    public class DataEntityToBusinessEntityMappingProfile : Profile
    {
        public override string ProfileName
        {
            get { return "DataEntityToBusinessEntityMapping"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<ReleaseWindow, BusinessEntities.ReleaseCalendar.ReleaseWindow>()
                .ForMember(target => target.Products, option => option.MapFrom(source => source.ReleaseProducts.Select(o => o.Product.Description)))
                .ForMember(target => target.StartTime,
                    option => option.MapFrom(source => source.StartTime.ToLocalTime()))
                .ForMember(target => target.OriginalStartTime,
                    option => option.MapFrom(source => source.OriginalStartTime.ToLocalTime()))
                .ForMember(target => target.ApprovedOn,
                    option =>
                        option.MapFrom(
                            source =>
                                source.Metrics.Any() && source.Metrics.Any(x => x.MetricType == MetricType.Approve && x.ExecutedOn.HasValue)
                                    ? source.Metrics.First(x => x.MetricType == MetricType.Approve).ExecutedOn.Value.ToLocalTime() : (DateTime?)null))
                .ForMember(target => target.ReleaseTypeDescription,
                    option => option.MapFrom(source => EnumDescriptionHelper.GetDescription(source.ReleaseType)))
                .ForMember(target => target.ReleaseDecision,
                    option => option.MapFrom(source => EnumDescriptionHelper.GetDescription(source.ReleaseDecision)))
                .ForMember(target => target.EndTime,
                    option =>
                        option.MapFrom(
                            source => source.Metrics.Any(x => x.MetricType == MetricType.EndTime && x.ExecutedOn.HasValue)
                                    ? source.Metrics.First(x => x.MetricType == MetricType.EndTime).ExecutedOn.Value.ToLocalTime() : (DateTime?)null))
                .ForMember(target => target.ClosedOn,
                    option =>
                        option.MapFrom(
                            source => source.Metrics.Any() && source.Metrics.Any(x => x.MetricType == MetricType.Close && x.ExecutedOn.HasValue)
                                    ? source.Metrics.First(x => x.MetricType == MetricType.Close).ExecutedOn.Value.ToLocalTime() : (DateTime?)null))
                .ForMember(target => target.SignedOff,
                    option =>
                        option.MapFrom(
                            source => source.Metrics.Any() && source.Metrics.Any(x => x.MetricType == MetricType.SignOff && x.ExecutedOn.HasValue)
                                    ? source.Metrics.First(x => x.MetricType == MetricType.SignOff).ExecutedOn.Value.ToLocalTime() : (DateTime?)null))
                .ForMember(target => target.Status, option => option.MapFrom(
                    source => !source.Metrics.Any()
                        ? EnumDescriptionHelper.GetDescription(ReleaseStatus.Opened)
                        : source.Metrics.Any(x => x.MetricType == MetricType.Close && x.ExecutedOn.HasValue)
                            ? EnumDescriptionHelper.GetDescription(ReleaseStatus.Closed)
                            : source.Metrics.Any(x => x.MetricType == MetricType.SignOff && x.ExecutedOn.HasValue)
                                ? EnumDescriptionHelper.GetDescription(ReleaseStatus.SignedOff)
                                : source.Metrics.Any(x => x.MetricType == MetricType.Approve && x.ExecutedOn.HasValue)
                                    ? EnumDescriptionHelper.GetDescription(ReleaseStatus.Approved)
                                    : EnumDescriptionHelper.GetDescription(ReleaseStatus.Opened)))
                .ForMember(target => target.Issues,
                    option =>
                        option.MapFrom(
                            source => source.Metrics.Any() && source.Metrics.Any(x => x.MetricType == MetricType.Approve && x.ExecutedOn.HasValue)
                                ? source.ReleaseNotes.Issues : null))
                .ForMember(target => target.ReleaseNotes, o => o.Ignore());

            Mapper.CreateMap<ReleaseWindow, BusinessEntities.ReleaseCalendar.ReleaseWindowView>()
                .ForMember(target => target.Products,
                    option => option.MapFrom(source => source.ReleaseProducts.Select(o => o.Product.Description)))
                .ForMember(target => target.StartTime,
                    option => option.MapFrom(source => source.StartTime.ToLocalTime()))
                .ForMember(target => target.EndTime,
                    option =>
                        option.MapFrom(
                            source =>
                                source.Metrics.Any(x => x.MetricType == MetricType.EndTime && x.ExecutedOn.HasValue)
                                    ? source.Metrics.First(x => x.MetricType == MetricType.EndTime)
                                        .ExecutedOn.Value.ToLocalTime()
                                    : (DateTime?) null))
                .ForMember(target => target.Status, option => option.MapFrom(
                    source => !source.Metrics.Any()
                        ? EnumDescriptionHelper.GetDescription(ReleaseStatus.Opened)
                        : source.Metrics.Any(x => x.MetricType == MetricType.Close && x.ExecutedOn.HasValue)
                            ? EnumDescriptionHelper.GetDescription(ReleaseStatus.Closed)
                            : source.Metrics.Any(x => x.MetricType == MetricType.SignOff && x.ExecutedOn.HasValue)
                                ? EnumDescriptionHelper.GetDescription(ReleaseStatus.SignedOff)
                                : source.Metrics.Any(x => x.MetricType == MetricType.Approve && x.ExecutedOn.HasValue)
                                    ? EnumDescriptionHelper.GetDescription(ReleaseStatus.Approved)
                                    : EnumDescriptionHelper.GetDescription(ReleaseStatus.Opened)))
                .ForMember(target => target.IsMaintenance,
                    options => options.MapFrom(source => source.ReleaseType.ToEnumDescription<ReleaseType, ReleaseTypeDescription>().IsMaintenance));
            
            Mapper.CreateMap<CommandExecution, BusinessEntities.ExecPoll.CommandExecution>()
                .ForMember(
                    target => target.State,
                    option => option.MapFrom(source => source.CommandHistory.LastOrDefault() != null ?
                        (BusinessEntities.ExecPoll.CommandStateType)((int)source.CommandHistory.Last().State) :
                        BusinessEntities.ExecPoll.CommandStateType.NotRegistered))
                .ForMember(
                    target => target.Details,
                    option => option.MapFrom(source => 
                        source.CommandHistory.LastOrDefault() != null ? source.CommandHistory.Last().Details : null));

            Mapper.CreateMap<Role, BusinessEntities.Auth.Role>();

            Mapper.CreateMap<Account, BusinessEntities.Auth.Account>()
                .ForMember(target => target.Products,
                option => option.MapFrom(source => source.AccountProducts == null || !source.AccountProducts.Any() ?
                    Enumerable.Empty<BusinessEntities.Products.ProductView>() :
                    source.AccountProducts.Select(Mapper.Map<AccountProduct, BusinessEntities.Products.ProductView>).ToArray()));

            Mapper.CreateMap<Session, BusinessEntities.Auth.Session>()
                .ForMember(target => target.AccountId, option => option.MapFrom(source => source.Account.ExternalId));

            Mapper.CreateMap<Event, BusinessEntities.Evt.Event>()
                .ForMember(target => target.State, option => option.MapFrom(source => source.EventHistory.LastOrDefault() != null ? source.EventHistory.Last().State : EventStateType.NotRegistered));

            Mapper.CreateMap<ReleaseTask, BusinessEntities.ReleasePlan.ReleaseTaskView>()
                .ForMember(target => target.ReleaseWindowId, option => option.MapFrom(x => x.ReleaseWindow.ExternalId))
                .ForMember(target => target.CreatedBy, option => option.MapFrom(source => source.CreatedBy.FullName))
                .ForMember(target => target.CompletedOn,
                    option =>
                        option.MapFrom(
                            source =>
                                source.CompletedOn != null ? source.CompletedOn.Value.ToLocalTime() : (DateTime?)null))
                .ForMember(target => target.Assignee, option => option.MapFrom(source => source.Assignee.FullName))
                .ForMember(target => target.AssigneeExternalId,
                    option => option.MapFrom(source => source.Assignee.ExternalId))
                .ForMember(target => target.ReceiptConfirmedOn,
                    option =>
                        option.MapFrom(
                            source => source.ReceiptConfirmedOn == null ? (DateTime?)null : source.ReceiptConfirmedOn.Value.ToLocalTime()))
                ;

            Mapper.CreateMap<ReleaseTask, BusinessEntities.ReleasePlan.ReleaseTask>()
                .ForMember(target => target.ReleaseWindowId, option => option.MapFrom(x => x.ReleaseWindow.ExternalId))
                .ForMember(target => target.CreatedBy, option => option.MapFrom(source => source.CreatedBy != null ? source.CreatedBy.FullName : null))
                .ForMember(target => target.CreatedByExternalId, option => option.MapFrom(source => source.CreatedBy != null ? source.CreatedBy.ExternalId : (Guid?)null))
                .ForMember(target => target.CreatedOn, option => option.MapFrom(source => source.CreatedOn.ToLocalTime()))
                .ForMember(target => target.CompletedOn, option => option.MapFrom(source => source.CompletedOn != null ? source.CompletedOn.Value.ToLocalTime() : (DateTime?)null))
                .ForMember(target => target.Assignee, option => option.MapFrom(source => source.Assignee != null ? source.Assignee.FullName : null))
                .ForMember(target => target.HelpDeskTicketReference, option => option.MapFrom(source => source.HelpDeskReference))
                .ForMember(target => target.HelpDeskTicketUrl, option => option.MapFrom(source => source.HelpDeskUrl))
                .ForMember(target => target.AssigneeExternalId, option => option.MapFrom(source => source.Assignee != null ? source.Assignee.ExternalId : (Guid?)null));

            Mapper.CreateMap<ReleaseTaskAttachment, BusinessEntities.ReleasePlan.ReleaseTaskAttachment>()
                .ForMember(x => x.ReleaseTaskId, o => o.Ignore());

            Mapper.CreateMap<ReleaseTaskAttachment, BusinessEntities.ReleasePlan.ReleaseTaskAttachmentView>()
                .ForMember(x => x.ServerName, o => o.Ignore())
                .ForMember(x => x.Size, o => o.MapFrom(source => source.Attachment.Length))
                .ForMember(x => x.Type, o => o.Ignore());

            Mapper.CreateMap<Product, BusinessEntities.Products.Product>();

            Mapper.CreateMap<BusinessUnit, BusinessEntities.Products.BusinessUnit>()
                .ForMember(target => target.Packages, option => option.Ignore());

            Mapper.CreateMap<AccountProduct, BusinessEntities.Products.ProductView>()
                .ForMember(target => target.Name, option => option.MapFrom(source => source.Product.Description))
                .ForMember(target => target.ExternalId, option => option.MapFrom(source => source.Product.ExternalId));

            Mapper.CreateMap<Product, BusinessEntities.Products.ProductView>()
                .ForMember(target => target.Name, option => option.MapFrom(source => source.Description));

            Mapper.CreateMap<ReleaseApprover, BusinessEntities.ReleasePlan.ReleaseApprover>()
                .ForMember(target => target.ApprovedOn, option => option.MapFrom(source => source.ApprovedOn.HasValue ? source.ApprovedOn.Value.ToLocalTime() : (DateTime?)null))
                .ForMember(target => target.Account, option => option.MapFrom(source => source.Account))
                .ForMember(target => target.ReleaseWindowId, option => option.MapFrom(source => source.ReleaseWindow.ExternalId));


            Mapper.CreateMap<ReleaseContent, BusinessEntities.ReleasePlan.ReleaseContentTicket>()
                .ForMember(target => target.TicketId, option => option.MapFrom(source => source.TicketId))
                .ForMember(target => target.Risk, option => option.MapFrom(source => source.TicketRisk))
                .ForMember(target => target.TicketDescription, option => option.MapFrom(source => source.Description))
                .ForMember(target => target.TicketName, option => option.MapFrom(source => source.TicketKey))
                .ForMember(target => target.LastChangedByAccount, option => option.MapFrom(source => source.LastChangedByAccount.ExternalId));

            Mapper.CreateMap<Command, BusinessEntities.Api.Command>()
                .ForMember(target => target.Description, option => option.MapFrom(source => source.Description))
                .ForMember(target => target.Group, option => option.MapFrom(source => source.Group))
                .ForMember(target => target.IsBackground, option => option.MapFrom(source => source.IsBackground))
                .ForMember(target => target.Name, option => option.MapFrom(source => source.Name))
                .ForMember(target => target.Roles, option => option.MapFrom(source => source.CommandPermissions.Select(x => x.Role)))
                .ForMember(target => target.HasRuleApplied, option => option.MapFrom(source => source.Rule != null))
                .ForMember(target => target.Description, option => option.MapFrom(source => source.Description));

            Mapper.CreateMap<Query, BusinessEntities.Api.Query>()
                .ForMember(target => target.Description, option => option.MapFrom(source => source.Description))
                .ForMember(target => target.Group, option => option.MapFrom(source => source.Group))
                .ForMember(target => target.IsStatic, option => option.MapFrom(source => source.IsStatic))
                .ForMember(target => target.Name, option => option.MapFrom(source => source.Name))
                .ForMember(target => target.Roles, option => option.MapFrom(source => source.QueryPermissions.Select(x => x.Role)))
                .ForMember(target => target.HasRuleApplied, option => option.MapFrom(source => source.Rule != null))
                .ForMember(target => target.Description, option => option.MapFrom(source => source.Description));

            Mapper.CreateMap<Role, BusinessEntities.Auth.Role>();
            Mapper.CreateMap<DataEntities.ReleaseExecution.SignOff, SignOff>()
                .ForMember(target => target.Signer, option => option.MapFrom(source => source.Account))
                .ForMember(target => target.SignedOff, option => option.MapFrom(source => source.SignedOff.HasValue));
            Mapper.CreateMap<Metric, BusinessEntities.Metrics.Metric>()
                .ForMember(target => target.ExecutedOn,
                    option =>
                        option.MapFrom(
                            source =>
                                source.ExecutedOn != null ? source.ExecutedOn.Value.ToLocalTime() : (DateTime?)null))
                .ForMember(target => target.Order,
                    option =>
                        option.MapFrom(
                            source => EnumDescriptionHelper.GetOrder(source.MetricType)));

            Mapper.CreateMap<ReleaseJob, BusinessEntities.DeploymentTool.ReleaseJob>();

            Mapper.CreateMap<ReleaseDeploymentMeasurement, JobMeasurement>()
                .ForMember(target => target.ChildSteps, option => option.Ignore())
                .ForMember(target => target.StartTime, option => option.MapFrom(source => source.StartTime != null ? source.StartTime.Value.ToLocalTime() : (DateTime?)null))
                .ForMember(target => target.FinishTime, option => option.MapFrom(source => source.FinishTime != null ? source.FinishTime.Value.ToLocalTime() : (DateTime?)null));

            Mapper.CreateMap<BusinessRuleParameter, BusinessEntities.BusinessRules.BusinessRuleParameter>();
            Mapper.CreateMap<BusinessRuleTestData, BusinessEntities.BusinessRules.BusinessRuleTestData>();
            Mapper.CreateMap<BusinessRuleDescription, BusinessEntities.BusinessRules.BusinessRuleDescription>();
            Mapper.CreateMap<BusinessRuleAccountTestData, BusinessEntities.BusinessRules.BusinessRuleAccountTestData>();
            Mapper.CreateMap<BusinessRuleDescription, BusinessEntities.BusinessRules.BusinessRuleView>()
                .ForMember(target => target.CodeBeggining,
                    option => option.MapFrom(source => string.IsNullOrEmpty(source.Script)
                        ? string.Empty
                        : source.Script.Length >= 30 ? source.Script.Substring(0, 30) + " ..." : source.Script));

            Mapper.CreateMap<ProductRequestGroupAssignee, BusinessEntities.Auth.Account>()
                .ConvertUsing(requestAssignee => Mapper.Map<Account, BusinessEntities.Auth.Account>(requestAssignee.Account));

            Mapper.CreateMap<ProductRequestType, BusinessEntities.ProductRequests.ProductRequestType>();
            Mapper.CreateMap<ProductRequestGroup, BusinessEntities.ProductRequests.ProductRequestGroup>()
                .ForMember(target => target.ProductRequestTypeId, option => option.MapFrom(source => source.RequestType.ExternalId))
                .ForMember(target => target.Assignees, option => option.MapFrom(source => source.Assignees));
            Mapper.CreateMap<ProductRequestTask, BusinessEntities.ProductRequests.ProductRequestTask>()
                .ForMember(target => target.ProductRequestGroupId, option => option.MapFrom(source => source.RequestGroup.ExternalId));

            Mapper.CreateMap<ProductRequestRegistrationTask, BusinessEntities.ProductRequests.ProductRequestRegistrationTask>()
                .ForMember(target => target.ProductRequestTaskId, option => option.MapFrom(source => source.ProductRequestTask.ExternalId))
                .ForMember(target => target.LastChangedByAccountId, option => option.MapFrom(source => source.LastChangedBy != null ? source.LastChangedBy.ExternalId : new Guid?()))
                .ForMember(target => target.LastChangedBy, option => option.MapFrom(source => source.LastChangedBy != null ? source.LastChangedBy.FullName : null))
                .ForMember(target => target.LastChangedOn, option => option.MapFrom(source => source.LastChangedOn.HasValue ? source.LastChangedOn.Value.ToLocalTime() : new DateTime?()));

            Mapper.CreateMap<ProductRequestRegistration, BusinessEntities.ProductRequests.ProductRequestRegistration>()
                .ForMember(target => target.ProductRequestTypeId, option => option.MapFrom(source => source.ProductRequestType.ExternalId))
                .ForMember(target => target.ProductRequestType, option => option.MapFrom(source => source.ProductRequestType.Name))
                .ForMember(target => target.CreatedByAccountId, option => option.MapFrom(source => source.CreatedBy.ExternalId))
                .ForMember(target => target.CreatedBy, option => option.MapFrom(source => source.CreatedBy.FullName))
                .ForMember(target => target.CreatedOn, option => option.MapFrom(source => source.CreatedOn.ToLocalTime()))
                .ForMember(target => target.Status, option => option.ResolveUsing<TasksToProductRequestRegistrationStatusResolver>().FromMember(source => source.Tasks));

            Mapper.CreateMap<PluginConfiguration, BusinessEntities.Plugins.GlobalPluginConfiguration>()
                .ForMember(target => target.PluginId, option => option.MapFrom(source => source.Plugin != null ? source.Plugin.ExternalId : (Guid?)null));

            Mapper.CreateMap<PluginPackageConfiguration, BusinessEntities.Plugins.PackagePluginConfiguration>()
                .ForMember(target => target.PackageId, option => option.MapFrom(source => source.Package.ExternalId))
                .ForMember(target => target.PackageName, option => option.MapFrom(source => source.Package.Description))
                .ForMember(target => target.BusinessUnit, option => option.MapFrom(source => source.Package.BusinessUnit.Description))
                .ForMember(target => target.PluginId, option => option.MapFrom(source => source.Plugin != null ? source.Plugin.ExternalId : (Guid?)null));

            Mapper.CreateMap<DataEntities.Plugins.Plugin, BusinessEntities.Plugins.Plugin>()
                .ForMember(target => target.PluginId, option => option.MapFrom(source => source.ExternalId))
                .ForMember(target => target.PluginKey, option => option.MapFrom(source => source.Key))
                .ForMember(target => target.PluginTypes,
                    option => option.MapFrom(source => source.PluginType.ToFlagList()));
        }
    }


}
