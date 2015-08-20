using AutoMapper;
using ReMi.Contracts.Plugins.Data.SourceControl;
using ReMi.Plugin.Gerrit.GerritApi.Model;
using ReMi.Plugin.Gerrit.Service.Model;

namespace ReMi.Plugin.Gerrit.AutoMapper
{
    public class GerritBusinessEntitiesToContractEntity : Profile
    {
        public override string ProfileName
        {
            get { return "GerritBusinessEntitiesToContractEntity"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<Repository, ReleaseRepository>()
                .ForMember(target => target.ChangesFrom, options => options.MapFrom(source => source.DefaultFrom))
                .ForMember(target => target.ChangesTo, options => options.MapFrom(source => source.DefaultTo))
                .ForMember(target => target.IsIncluded, options => options.MapFrom(source => source.IsIncludedByDefault))
                .ForMember(target => target.IsDisabled, options => options.MapFrom(source => source.IsDisabled))
                .ForMember(target => target.LatestChange, options => options.MapFrom(source => source.StartFromLatest))
                .ForMember(target => target.Repository, options => options.MapFrom(source => source.Name));
            Mapper.CreateMap<GitLogEntity, SourceControlChange>()
                .ForMember(target => target.Description, options => options.MapFrom(source => source.Subject))
                .ForMember(target => target.Identifier, options => options.MapFrom(source => source.Hash))
                .ForMember(target => target.Owner, options => options.MapFrom(source => source.Author))
                .ForMember(target => target.Date, options => options.MapFrom(source => source.Date));
        }
    }
}
