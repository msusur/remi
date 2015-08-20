using AutoMapper;
using ReMi.DataEntities.ReleasePlan;
using ReMi.DataEntities.SourceControl;

namespace ReMi.DataAccess.AutoMapper
{
    public class DataEntityToContractEntityMappingProfile : Profile
    {
        public override string ProfileName
        {
            get { return "DataEntityToContractEntityMappingProfile"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<SourceControlChange, Contracts.Plugins.Data.SourceControl.SourceControlChange>();
            Mapper.CreateMap<ReleaseRepository, Contracts.Plugins.Data.SourceControl.ReleaseRepository>()
                .ForMember(target => target.Repository, option => option.MapFrom(x => x.Name))
                .ForMember(target => target.ExternalId, option => option.MapFrom(x => x.RepositoryId))
                .ForMember(target => target.IsDisabled, option => option.UseValue(false));
        }
    }


}
