using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Plugin.Ldap.DataAccess.Setup;

namespace ReMi.Plugin.EmailMock.DataAccess.DataEntities
{
    [Table("GlobalConfiguration", Schema = Constants.Schema)]
    public class GlobalConfiguration
    {
        [Key]
        public int GlobalConfigurationId { get; set; }

        public string RedirectToEmail { get; set; }
    }
}
