using AutoMapper;
using ReMi.Plugin.EmailMock.DataAccess.DataEntities;

namespace ReMi.Plugin.EmailMock.AutoMapper
{
    public class EmailMockBusinessEnitiesToDataEntities : Profile
    {
        public override string ProfileName
        {
            get { return "EmailMockBusinessEnitiesToDataEntities"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<PluginConfigurationEntity, GlobalConfiguration>();
        }
    }
}
