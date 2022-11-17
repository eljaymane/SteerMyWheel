using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using Renci.SshNet.Common;
using SteerMyWheel.Configuration;
using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Core.Model.Enums;
using SteerMyWheel.Domain.Connectivity.ClientProvider;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Connectivity.ClientProviders
{
    public class SSHClientProvider : BaseClientProvider<SshClient, RemoteHost>
    {
        private SshClient _client;
        private readonly GlobalConfig _config;
        private readonly ILogger<SSHClientProvider> _logger;

        public SSHClientProvider(GlobalConfig config,ILogger<SSHClientProvider> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<string> getCronFile()
        {
            if (!isConnected()) return null;
            else
            {
                _logger.LogInformation("[{time}] Executing 'crontab -l' on RemoteHost : {ip} ...",DateTime.UtcNow, _client.ConnectionInfo.Host);
                try
                {
                    var cmd = _client.CreateCommand("crontab -l");
                    var result = cmd.Execute();
                    return result;
                }catch(Exception e)
                {

                }
                finally 
                {
                    _logger.LogInformation("[{time}] Successfully read cronfile for user {user} on RemoteHost : {ip} ...", DateTime.UtcNow,_client.ConnectionInfo.Username, _client.ConnectionInfo.Host);
                }

                return String.Empty;
            }

        }

        public override Task Connect(RemoteHost host)
        {
            try
            {
                var privateKey = host.ConnectionMethod != SSHConnectionMethod.DEFAULT ? new PrivateKeyFile(new MemoryStream(File.ReadAllBytes(_config.SSHKeysPATH + host.Name))) : null;
                _client = privateKey == null ? new SshClient(host.RemoteIP, host.SSHPort, host.SSHUsername, host.SSHPassword) : new SshClient(host.RemoteIP, host.SSHPort, host.SSHUsername, privateKey);

                _logger.LogInformation("[{time}] Connecting to RemoteHost : {ip} ...", DateTime.UtcNow,host.RemoteIP);
                _client.Connect();
            }
            catch(FileNotFoundException e)
            {
                _logger.LogError("[{time}] Could not find private key file for RemoteHost : {ip} ...", DateTime.UtcNow, host.RemoteIP);
                return Task.FromException(e);   
            }catch(SshAuthenticationException e)
            {
                _logger.LogError("[{time}] Could not find authenticate to RemoteHost : {ip} ...", DateTime.UtcNow, host.RemoteIP);
                return Task.FromException(e);
            }catch(SshConnectionException e)
            {
                _logger.LogError("[{time}] Could not connect to RemoteHost : {ip} ! Check your network and try again ...", DateTime.UtcNow, host.RemoteIP);
            }
            catch(Exception e)
            {
                _logger.LogError($"[{DateTime.UtcNow}] An unhandled exception occured, quitting...");
            }
            finally
            {
                _logger.LogInformation("[{time}] Successfully connected to RemoteHost : {ip} ...", DateTime.UtcNow, host.RemoteIP);
            }

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
            _client?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
