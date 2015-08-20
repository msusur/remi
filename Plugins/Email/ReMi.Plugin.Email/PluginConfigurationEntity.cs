using System.Configuration;
using ReMi.Contracts.Plugins.Data;

namespace ReMi.Plugin.Email
{
    public class PluginConfigurationEntity : IPluginConfigurationEntity
    {
        public PluginConfigurationEntity()
        {
            UserName = ConfigurationManager.AppSettings["RemiUserEmail"];
            Password = ConfigurationManager.AppSettings["RemiPassword"];
            Smtp = ConfigurationManager.AppSettings["SmtpHost"];
        }

        [PluginConfiguration("User Name", PluginConfigurationType.String)]
        public string UserName { get; set; }
        [PluginConfiguration("Password", PluginConfigurationType.Password)]
        public string Password { get; set; }
        [PluginConfiguration("SMTP", PluginConfigurationType.String)]
        public string Smtp { get; set; }
    }
}
