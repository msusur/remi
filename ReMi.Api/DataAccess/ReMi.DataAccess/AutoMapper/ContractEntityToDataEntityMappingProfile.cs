using System;
using AutoMapper;
using ReMi.Contracts.Plugins.Data.SourceControl;

namespace ReMi.DataAccess.AutoMapper
{
    public class ContractEntityToDataEntityMappingProfile : Profile
    {
        public override string ProfileName
        {
            get { return "ContractEntityToDataEntityMappingProfile"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<SourceControlChange, DataEntities.SourceControl.SourceControlChange>()
                .ForMember(target => target.Description,
                    option => option.MapFrom(
                        source => source.Description != null && source.Description.Length >= 2048 ? source.Description.Substring(0, 2048) : source.Description));
            Mapper.CreateMap<ReleaseRepository, DataEntities.ReleasePlan.ReleaseRepository>()
                .ForMember(target => target.Name, option => option.MapFrom(x => x.Repository))
                .ForMember(target => target.RepositoryId, option => option.MapFrom(x => x.ExternalId));
        }
    }
}
