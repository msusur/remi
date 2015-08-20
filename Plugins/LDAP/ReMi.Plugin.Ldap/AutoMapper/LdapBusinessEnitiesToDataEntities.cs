using AutoMapper;
using ReMi.Plugin.Ldap.DataAccess.DataEntities;

namespace ReMi.Plugin.Ldap.AutoMapper
{
    public class LdapBusinessEnitiesToDataEntities : Profile
    {
        public override string ProfileName
        {
            get { return "LdapBusinessEnitiesToDataEntities"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<PluginConfigurationEntity, GlobalConfiguration>();
        }
    }
}
