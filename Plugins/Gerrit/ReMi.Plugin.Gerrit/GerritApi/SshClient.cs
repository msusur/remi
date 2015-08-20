using System;
using System.IO;
using System.Text;
using ReMi.Plugin.Gerrit.DataAccess.Gateways;
using Renci.SshNet;

namespace ReMi.Plugin.Gerrit.GerritApi
{
    public class SshClient : ISshClient
    {
        private Renci.SshNet.SshClient _sshClient;

        public Func<IGlobalConfigurationGateway> GlobalConfigurationGatewayFactory { get; set; }

        public void Connect()
        {
            PluginConfigurationEntity configuration;
            using (var gateway = GlobalConfigurationGatewayFactory())
            {
                configuration = gateway.GetGlobalConfiguration();
            }

            var keyStream = new MemoryStream(Encoding.UTF8.GetBytes(configuration.PrivateKey));
            var connectionInfo = new ConnectionInfo(configuration.Host, configuration.Port, configuration.User,
                new PrivateKeyAuthenticationMethod(configuration.User, new PrivateKeyFile(keyStream)));
            _sshClient = new Renci.SshNet.SshClient(connectionInfo);
            _sshClient.Connect();
        }

        public string ExecuteCommand(string commandText)
        {
            var command = _sshClient.CreateCommand(commandText);
            return command.Execute();
        }

        public void Dispose()
        {
            if (_sshClient == null || !_sshClient.IsConnected) return;
            _sshClient.Disconnect();
            _sshClient.Dispose();
        }
    }
}
