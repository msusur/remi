using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Plugin.Gerrit.DataAccess.Setup;

namespace ReMi.Plugin.Gerrit.DataAccess.DataEntities
{
    [Table("GlobalConfiguration", Schema = Constants.Schema)]
    public class GlobalConfiguration
    {
        [Key]
        public int GlobalConfigurationId { get; set; }

        public string Host { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string PrivateKey { get; set; }
    }
}
