using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Plugin.Jenkins.DataAccess.Setup;

namespace ReMi.Plugin.Jenkins.DataAccess.DataEntities
{
    [Table("GlobalConfiguration", Schema = Constants.Schema)]
    public class GlobalConfiguration
    {
        [Key]
        public int GlobalConfigurationId { get; set; }

        public string JenkinsUser { get; set; }
        public string JenkinsPassword { get; set; }
        public virtual ICollection<JenkinsServerConfiguration> JenkinsServers { get; set; }
    }
}
