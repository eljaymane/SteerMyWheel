using Renci.SshNet;
using SteerMyWheel.Model;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SteerMyWheel.Connectivity.ClientProviders
{
    public class SSHClientProvider : BaseClientProvider<SshClient, RemoteHost>
    {
        private SshClient _client;
        private readonly GlobalConfig _config;

        public SSHClientProvider(GlobalConfig config)
        {
            _config = config;
        }

        public async Task<string> getCronFile()
        {
            if (!isConnected()) return null;
            else
            {
                var cmd = _client.CreateCommand("crontab -l");
                cmd.Execute();

                var reader = new StreamReader(cmd.ExtendedOutputStream);
                var cron = await reader.ReadToEndAsync();
                return cron;
            }

        }

        public override Task Connect(RemoteHost host)
        {
            _client = new SshClient(new ConnectionInfo(host.RemoteIP, host.SSHUsername));
            _client.Connect();
            return Task.CompletedTask;
        }

        public override SshClient GetConnection()
        {
            return _client;
        }

        public override bool isConnected()
        {
            return _client.IsConnected;
        }

        public override void Dispose()
        {
            _client.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
