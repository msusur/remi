using AutoMapper;
using ReMi.Plugin.EmailMock.DataAccess.DataEntities;

namespace ReMi.Plugin.EmailMock.AutoMapper
{
    public class EmailMockDataEntitiesToBusinessEnities : Profile
    {
        public override string ProfileName
        {
            get { return "EmailMockDataEntitiesToBusinessEnities"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<GlobalConfiguration, PluginConfigurationEntity>();
        }
    }
}
