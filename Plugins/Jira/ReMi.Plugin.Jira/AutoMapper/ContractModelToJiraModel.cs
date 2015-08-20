using AutoMapper;
using ReMi.Contracts.Plugins.Data.ReleaseContent;
using ReMi.Plugin.Jira.Models;

namespace ReMi.Plugin.Jira.AutoMapper
{
    public class ContractModelToJiraModel : Profile
    {
        public override string ProfileName
        {
            get { return "ContractModelToJiraModel"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<ReleaseContentTicket, Issue>()
                .ForMember(target => target.Fields, option => option.MapFrom(source => new IssueFields
                {
                    Summary = source.TicketName,
                    Assignee = new User { DisplayName = source.Assignee }
                }))
                .ForMember(target => target.Key, option => option.MapFrom(source => source.TicketName))
                .ForMember(target => target.Id,
                    option => option.MapFrom(source => System.BitConverter.ToInt32(source.TicketId.ToByteArray(), 0)));
        }
    }
}
