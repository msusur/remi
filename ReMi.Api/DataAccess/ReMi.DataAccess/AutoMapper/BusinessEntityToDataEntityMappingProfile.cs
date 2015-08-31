using AutoMapper;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.BusinessRules;
using ReMi.BusinessEntities.DeploymentTool;
using ReMi.BusinessEntities.ProductRequests;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessEntities.ReleasePlan;
using System;
using ReMi.BusinessEntities.Products;

namespace ReMi.DataAccess.AutoMapper
{
    public class BusinessEntityToDataEntityMappingProfile : Profile
    {
        public override string ProfileName
        {
            get { return "BusinessEntityToDataEntityMapping"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<ReleaseWindow, DataEntities.ReleaseCalendar.ReleaseWindow>()
                .ForMember(target => target.ReleaseProducts, option => option.Ignore())
                .ForMember(target => target.StartTime,
                    option => option.MapFrom(source => source.StartTime.ToUniversalTime()))
                .ForMember(target => target.OriginalStartTime,
                    option => option.MapFrom(source => source.OriginalStartTime.ToUniversalTime()));

            Mapper.CreateMap<BusinessEntities.ExecPoll.CommandExecution, DataEntities.ExecPoll.CommandExecution>()
                .ForMember(target => target.CommandHistory, option => option.Ignore());

            Mapper.CreateMap<Role, DataEntities.Auth.Role>();

            Mapper.CreateMap<Account, DataEntities.Auth.Account>()
                .ForMember(target => target.AccountId, option => option.Ignore())
                .ForMember(target => target.AccountProducts, option => option.Ignore());

            Mapper.CreateMap<Session, DataEntities.Auth.Session>()
                .ForMember(target => target.SessionId, option => option.Ignore())
                .ForMember(target => target.Account, option => option.Ignore());

            Mapper.CreateMap<BusinessEntities.Evt.Event, DataEntities.Evt.Event>()
                .ForMember(target => target.EventHistory, option => option.Ignore());

            Mapper.CreateMap<ReleaseTaskView, DataEntities.ReleasePlan.ReleaseTask>()
                .ForMember(target => target.CreatedBy, option => option.Ignore())
                .ForMember(target => target.Assignee, option => option.Ignore())
                .ForMember(target => target.ReleaseWindowId, option => option.Ignore())
                .ForMember(target => target.Attachments, option => option.Ignore())
                ;

            Mapper.CreateMap<ReleaseTask, DataEntities.ReleasePlan.ReleaseTask>()
                .ForMember(target => target.CreatedBy, option => option.Ignore())
                .ForMember(target => target.Assignee, option => option.Ignore())
                .ForMember(target => target.ReleaseWindowId, option => option.Ignore())
                .ForMember(target => target.Attachments, option => option.Ignore())
                ;

            Mapper.CreateMap<ReleaseTaskAttachment, DataEntities.ReleasePlan.ReleaseTaskAttachment>()
                .ForMember(x => x.ReleaseTaskId, o => o.Ignore());

            Mapper.CreateMap<ReleaseApprover, DataEntities.ReleasePlan.ReleaseApprover>()
                .ForMember(target => target.ApprovedOn, option => option.MapFrom(source => source.ApprovedOn.HasValue ? source.ApprovedOn.Value.ToUniversalTime() : (DateTime?)null))
                .ForMember(target => target.ReleaseApproverId, option => option.Ignore())
                .ForMember(target => target.ReleaseWindowId, option => option.Ignore())
                .ForMember(target => target.Account, option => option.Ignore())
                .ForMember(target => target.AccountId, option => option.Ignore());


            Mapper.CreateMap<ReleaseContentTicket, DataEntities.ReleasePlan.ReleaseContent>()
                .ForMember(target => target.TicketId, option => option.MapFrom(source => source.TicketId))
                .ForMember(target => target.TicketRisk, option => option.MapFrom(source => source.Risk))
                .ForMember(target => target.Description, option => option.MapFrom(source => source.TicketDescription))
                .ForMember(target => target.TicketKey, option => option.MapFrom(source => source.TicketName))
                .ForMember(target => target.LastChangedByAccount, option => option.MapFrom(source => new DataEntities.Auth.Account { ExternalId = source.LastChangedByAccount }));

            Mapper.CreateMap<Account, DataEntities.Auth.Account>();

            Mapper.CreateMap<Role, DataEntities.Auth.Role>();

            Mapper.CreateMap<Account, DataEntities.ProductRequests.ProductRequestGroupAssignee>()
                .ForMember(target => target.AccountId, options => options.Ignore())
                .ForMember(target => target.ProductRequestGroupAssigneeId, options => options.Ignore())
                .ForMember(target => target.ProductRequestGroupId, options => options.Ignore())
                .ForMember(target => target.Account, options =>
                    options.MapFrom(source =>
                        new Account
                        {
                            ExternalId = source.ExternalId,
                            FullName = source.FullName
                        }));
            Mapper.CreateMap<ProductRequestType, DataEntities.ProductRequests.ProductRequestType>()
                .ForMember(target => target.RequestGroups, options => options.Ignore())
                .ForMember(target => target.ProductRequestTypeId, options => options.Ignore())
                .ForMember(target => target.RequestGroups, options => options.Ignore());
            Mapper.CreateMap<ProductRequestGroup, DataEntities.ProductRequests.ProductRequestGroup>()
                .ForMember(target => target.ProductRequestGroupId, options => options.Ignore())
                .ForMember(target => target.ProductRequestTypeId, options => options.Ignore())
                .ForMember(target => target.Assignees, options => options.Ignore())
                .ForMember(target => target.RequestTasks, options => options.Ignore())
                .ForMember(target => target.RequestType, options => options.Ignore());
            Mapper.CreateMap<ProductRequestTask, DataEntities.ProductRequests.ProductRequestTask>()
                .ForMember(target => target.ProductRequestGroupId, options => options.Ignore())
                .ForMember(target => target.ProductRequestTaskId, options => options.Ignore())
                .ForMember(target => target.RequestGroup, options => options.Ignore());

            Mapper.CreateMap<BusinessRuleParameter, DataEntities.BusinessRules.BusinessRuleParameter>();
            Mapper.CreateMap<BusinessRuleTestData, DataEntities.BusinessRules.BusinessRuleTestData>();
            Mapper.CreateMap<BusinessRuleAccountTestData, DataEntities.BusinessRules.BusinessRuleAccountTestData>();
            Mapper.CreateMap<BusinessRuleDescription, DataEntities.BusinessRules.BusinessRuleDescription>();

            Mapper.CreateMap<ProductRequestRegistration, DataEntities.ProductRequests.ProductRequestRegistration>()
                .ForMember(target => target.ProductRequestTypeId, options => options.Ignore())
                .ForMember(target => target.ProductRequestType, options => options.Ignore())
                .ForMember(target => target.Tasks, options => options.Ignore())
                .ForMember(target => target.ProductRequestRegistrationId, options => options.Ignore())
                .ForMember(target => target.CreatedByAccountId, options => options.Ignore())
                .ForMember(target => target.CreatedOn, option => option.MapFrom(source => source.CreatedOn.ToUniversalTime()))
                .ForMember(target => target.CreatedBy, options => options.Ignore());

            Mapper.CreateMap<ProductRequestRegistrationTask, DataEntities.ProductRequests.ProductRequestRegistrationTask>()
                .ForMember(target => target.ProductRequestTaskId, options => options.Ignore())
                .ForMember(target => target.LastChangedByAccountId, options => options.Ignore())
                .ForMember(target => target.LastChangedOn, option => option.Ignore())
                .ForMember(target => target.LastChangedBy, options => options.Ignore())
                .ForMember(target => target.ProductRequestTask, options => options.Ignore())
                .ForMember(target => target.ProductRequestRegistrationId, options => options.Ignore());

            Mapper.CreateMap<ReleaseJob, DataEntities.ReleasePlan.ReleaseJob>();

            Mapper.CreateMap<BusinessUnit, DataEntities.Products.BusinessUnit>()
                .ForMember(target => target.Packages, options => options.Ignore());
        }
    }
}
