using AutoMapper;
using ReMi.Plugin.Ldap.DataAccess.DataEntities;

namespace ReMi.Plugin.Ldap.AutoMapper
{
    public class LdapDataEntitiesToBusinessEnities : Profile
    {
        public override string ProfileName
        {
            get { return "LdapDataEntitiesToBusinessEnities"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<GlobalConfiguration, PluginConfigurationEntity>();
        }
    }
}
