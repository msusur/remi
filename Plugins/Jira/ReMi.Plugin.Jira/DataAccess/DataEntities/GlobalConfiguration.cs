using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Plugin.Jira.DataAccess.Setup;

namespace ReMi.Plugin.Jira.DataAccess.DataEntities
{
    [Table("GlobalConfiguration", Schema = Constants.Schema)]
    public class GlobalConfiguration
    {
        [Key]
        public int GlobalConfigurationId { get; set; }

        public string JiraUrl { get; set; }
        public string JiraBrowseUrl { get; set; }
        public int JiraIssuesMaxCount { get; set; }
        public string JiraUser { get; set; }
        public string JiraPassword { get; set; }
    }
}
