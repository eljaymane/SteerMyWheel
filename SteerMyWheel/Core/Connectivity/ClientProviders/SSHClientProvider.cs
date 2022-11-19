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
        private SftpClient _sftpClient;
        private readonly GlobalConfig _config;
        private readonly ILogger<SSHClientProvider> _logger;

        public SSHClientProvider(GlobalConfig config,ILogger<SSHClientProvider> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<string> GetCronFile()
        {
            if (!isConnected()) return null;
            else
            {
                _logger.LogInformation("[{time}] [SSH] Executing 'crontab -l' on RemoteHost : {ip} ...", DateTime.UtcNow, _client.ConnectionInfo.Host);
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
                    _logger.LogInformation("[{time}] [SSH] Successfully read cronfile for user {user} on RemoteHost : {ip} ...", DateTime.UtcNow,_client.ConnectionInfo.Username, _client.ConnectionInfo.Host);
                }

                return String.Empty;
            }

        }

        public async Task DownloadRepository(ScriptRepository repository,string localPath)
        {
            if (!_sftpClient.IsConnected) return;
            else
            {
                var localDirectory = _config.LocalWorkingDirectory + repository.Name;
                var files = _sftpClient.ListDirectory(repository.Path);
                _logger.LogInformation($"[{DateTime.UtcNow}] [SFTP] Downloading sources of {repository.Name} from server {_sftpClient.ConnectionInfo.Host} ... ");
                foreach (var file in files)
                {
                    if(file.Name != "." && file.Name != "..")
                    {
                        if (file.IsDirectory && !ParserConfig.IgnoreDirectories.Contains(file.Name))
                        {
                            await DownloadRepository(repository, localPath + "/" + file.Name +"/");
                        } else
                        {
                            if (File.Exists(localPath + file.Name)) File.Move(localPath + file.Name, localPath + file.Name + ".bak");
                            using (Stream fileStream = File.Create(localPath + file.Name))
                            {
                                _sftpClient.DownloadFile(repository.Path + file.Name, fileStream);
                            }
                        }
                    }
                }
            }
        }

        public override Task ConnectSSH(RemoteHost host)
        {
            try
            {
                var privateKey = host.ConnectionMethod != SSHConnectionMethod.DEFAULT ? new PrivateKeyFile(new MemoryStream(File.ReadAllBytes(_config.SSHKeysPATH + host.Name))) : null;
                _client = privateKey == null ? new SshClient(host.RemoteIP, host.SSHPort, host.SSHUsername, host.SSHPassword) : new SshClient(host.RemoteIP, host.SSHPort, host.SSHUsername, privateKey);

                _logger.LogInformation("[{time}] [SSH] Connecting to RemoteHost : {ip} ...", DateTime.UtcNow,host.RemoteIP);
                _client.Connect();
            }
            catch(FileNotFoundException e)
            {
                _logger.LogError("[{time}] [SSH] Could not find private key file for RemoteHost : {ip} ...", DateTime.UtcNow, host.RemoteIP);
                return Task.FromException(e);   
            }catch(SshAuthenticationException e)
            {
                _logger.LogError("[{time}] [SSH] Could not find authenticate to RemoteHost : {ip} ...", DateTime.UtcNow, host.RemoteIP);
                return Task.FromException(e);
            }catch(SshConnectionException e)
            {
                _logger.LogError("[{time}] [SSH] Could not connect to RemoteHost : {ip} ! Check your network and try again ...", DateTime.UtcNow, host.RemoteIP);
            }
            catch(Exception e)
            {
                _logger.LogError($"[{DateTime.UtcNow}] [SSH] An unhandled exception occured, quitting...");
            }
            finally
            {
                _logger.LogInformation("[{time}] [SSH] Successfully connected to RemoteHost : {ip} ...", DateTime.UtcNow, host.RemoteIP);
            }

            return Task.CompletedTask;
        }

        public Task ConnectSFTP(RemoteHost host)
        {
            try
            {
                var privateKey = host.ConnectionMethod != SSHConnectionMethod.DEFAULT ? new PrivateKeyFile(new MemoryStream(File.ReadAllBytes(_config.SSHKeysPATH + host.Name))) : null;
                _sftpClient = privateKey == null ? new SftpClient(host.RemoteIP, host.SSHPort, host.SSHUsername, host.SSHPassword) : new SftpClient(host.RemoteIP, host.SSHPort, host.SSHUsername, privateKey);

                _logger.LogInformation("[{time}] [SFTP]  Connecting to RemoteHost : {ip} ...", DateTime.UtcNow, host.RemoteIP);
                _sftpClient.Connect();
            }
            catch (FileNotFoundException e)
            {
                _logger.LogError("[{time}] [SFTP]  Could not find private key file for RemoteHost : {ip} ...", DateTime.UtcNow, host.RemoteIP);
                return Task.FromException(e);
            }
            catch (SshAuthenticationException e)
            {
                _logger.LogError("[{time}] [SFTP]  Could not find authenticate to RemoteHost : {ip} ...", DateTime.UtcNow, host.RemoteIP);
                return Task.FromException(e);
            }
            catch (SshConnectionException e)
            {
                _logger.LogError("[{time}] [SFTP]  Could not connect to RemoteHost : {ip} ! Check your network and try again ...", DateTime.UtcNow, host.RemoteIP);
            }
            catch (Exception e)
            {
                _logger.LogError($"[{DateTime.UtcNow}] [SFTP]  An unhandled exception occured, quitting...");
            }
            finally
            {
                _logger.LogInformation("[{time}] [SFTP]  Successfully connected to RemoteHost : {ip} ...", DateTime.UtcNow, host.RemoteIP);
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
