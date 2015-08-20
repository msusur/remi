using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Plugin.Jenkins.DataAccess.Setup;

namespace ReMi.Plugin.Jenkins.DataAccess.DataEntities
{
    [Table("JenkinsServerConfiguration", Schema = Constants.Schema)]
    public class JenkinsServerConfiguration
    {
        [Key]
        public int JenkinsServerConfigurationId { get; set; }

        [Index(IsUnique = true), StringLength(256)]
        public string Name { get; set; }

        public string Value { get; set; }

        public int GlobalConfigurationId { get; set; }
        public virtual GlobalConfiguration GlobalConfiguration { get; set; }
    }
}
