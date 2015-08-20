using AutoMapper;
using ReMi.Contracts.Plugins.Data.ReleaseContent;
using ReMi.Contracts.Plugins.Services;
using ReMi.Plugin.Jira.Models;

namespace ReMi.Plugin.Jira.AutoMapper
{
    public class JiraModelToContractContract : Profile
    {
        public override string ProfileName
        {
            get { return "JiraModelToContractContract"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<Issue, ReleaseContentTicket>()
                .ForMember(target => target.TicketDescription, option => option.MapFrom(source => source.Fields.Summary))
                .ForMember(target => target.TicketName, option => option.MapFrom(source => source.Key))
                .ForMember(target => target.TicketId, option => option.MapFrom(source => new System.Guid(source.Id, 1, 3, new byte[] { 1, 0, 0, 0, 0, 0, 0, 1 })))
                .ForMember(target => target.Assignee, option => option.MapFrom(source => source.Fields.Assignee != null ? source.Fields.Assignee.DisplayName : "unassigned"));
        }
    }
}
