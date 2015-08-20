using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Plugin.Go.DataAccess.Setup;

namespace ReMi.Plugin.Go.DataAccess.DataEntities
{
    [Table("GlobalConfiguration", Schema = Constants.Schema)]
    public class GlobalConfiguration
    {
        [Key]
        public int GlobalConfigurationId { get; set; }

        public string GoUser { get; set; }
        public string GoPassword { get; set; }
        public virtual ICollection<GoServerConfiguration> GoServerConfiguration { get; set; }
    }
}
