using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Plugin.Ldap.DataAccess.Setup;

namespace ReMi.Plugin.Ldap.DataAccess.DataEntities
{
    [Table("GlobalConfiguration", Schema = Constants.Schema)]
    public class GlobalConfiguration
    {
        [Key]
        public int GlobalConfigurationId { get; set; }

        public string UserName { get; set; }
        public string Password { get; set; }
        public string LdapPath { get; set; }
        public string SearchCriteria { get; set; }
    }
}
