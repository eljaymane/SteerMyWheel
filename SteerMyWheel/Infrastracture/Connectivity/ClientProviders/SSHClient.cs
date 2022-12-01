using Microsoft.Extensions.Logging;
using Renci.SshNet;
using Renci.SshNet.Common;
using SteerMyWheel.Configuration;
using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Core.Model.Enums;
using SteerMyWheel.Infrastracture.Connectivity.ClientProviders.Exceptions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SteerMyWheel.Infrastracture.Connectivity.ClientProviders
{
    public class SSHClient : BaseClientProvider<SshClient, RemoteHost>
    {
        private SshClient _client;
        private SftpClient _sftpClient;
        private readonly GlobalConfig _config;
        private readonly ILogger<SSHClient> _logger;


        public SSHClient(GlobalConfig config, ILogger<SSHClient> logger)
        {
            _config = config;
            _logger = logger;
        }

        public bool FileExists(string path)
        {
            if (!_sftpClient.IsConnected) throw new SSHClientNotConnectedException();
            return _sftpClient.Exists(path);
        }

        public bool DirectoryExists(string path)
        {
            if(!_sftpClient.IsConnected) throw new SSHClientNotConnectedException();
            return _sftpClient.Exists(path);
        }

        public Task<string> GetCronFile()
        {
            if (!isConnected()) return null;
            else
            {
                _logger.LogInformation("[{time}] [SSH] Executing 'crontab -l' on RemoteHost : {ip} ...", DateTime.UtcNow, _client.ConnectionInfo.Host);
                try
                {
                    var cmd = _client.CreateCommand("crontab -l");
                    var result = cmd.Execute();
                    return Task.FromResult(result);
                }
                catch (Exception e)
                {

                }
                finally
                {
                    _logger.LogInformation("[{time}] [SSH] Successfully read cronfile for user {user} on RemoteHost : {ip} ...", DateTime.UtcNow, _client.ConnectionInfo.Username, _client.ConnectionInfo.Host);
                }

                return Task.FromResult(string.Empty);
            }

        }

        public Task<bool> ExecuteCmd(string cmd)
        {
            if (_client.IsConnected) _logger.LogInformation($"[{DateTime.UtcNow}] Trying to execute command {cmd} on remote host : {_client.ConnectionInfo.Host}");
            else
            {
                _logger.LogInformation($"[{DateTime.UtcNow}] Client not connected...");
                return Task.FromResult(false);
            }
            var result = _client.CreateCommand(cmd + " && echo success:true").Execute();
            if (result.Contains("success:true"))
            {
                _logger.LogInformation($"[{DateTime.UtcNow}] Command {cmd} has been executed successfully on remote host : {_client.ConnectionInfo.Host}");
                return Task.FromResult(true);
            }
            else
            {
                _logger.LogError($"[{DateTime.UtcNow}] Command {cmd} has not completed successfully on remote host : {_client.ConnectionInfo.Host}");
                return Task.FromResult(false);
            }




        }

        public async Task DownloadDirectory(string remotePath, string localPath)
        {

            if (!_sftpClient.IsConnected || remotePath.Trim() == "") return;
            else
            {

                var files = _sftpClient.ListDirectory(remotePath);
                _logger.LogInformation($"[{DateTime.UtcNow}] [SFTP] Downloading {remotePath} from server {_sftpClient.ConnectionInfo.Host} ... ");
                foreach (var file in files)
                {
                    if (file.Name != "." && file.Name != "..")
                    {
                        if (file.IsDirectory && !ParserConfig.IgnoreDirectories.Contains(file.Name))
                        {
                            if (localPath == "") return;
                            await DownloadDirectory(remotePath + file.Name, localPath + "\\" + file.Name);
                        }
                        else if (!file.Name.Contains(".csv"))
                        {
                            if (file.Name.StartsWith(".")) return;
                            if (File.Exists(localPath + "\\" + file.Name))
                            {
                                if (File.Exists(localPath + "\\" + file.Name + ".bak")) File.Delete(localPath + "\\" + file.Name + ".bak");
                                File.Move(localPath + "\\" + file.Name, localPath + "\\" + file.Name + ".bak");
                                File.Delete(localPath + "\\" + file.Name);
                            }
                            using (Stream fileStream = File.Create(localPath + "\\" + file.Name))
                            {
                                try
                                {
                                    _sftpClient.DownloadFile(remotePath + file.Name, fileStream);
                                    _logger.LogInformation($"[{DateTime.UtcNow}] Successfully downloaded file {file.Name} to {localPath + "\\" + file.Name} !");
                                }
                                catch (Exception e)
                                {
                                    _logger.LogError($"[{DateTime.UtcNow}] Could not download {file.Name} reason : {e.Message} ...");
                                }

                            }
                        }
                    }
                }
            }
        }

        public async Task Upload(string remotePath, string localPath)
        {
            if (!_sftpClient.IsConnected) return;
            try
            {
                bool isDir = Directory.Exists(localPath);
                bool isFile = File.Exists(localPath);
                if (isDir)
                {
                    _logger.LogInformation($"[{DateTime.UtcNow}] Upload : Localpath {localPath} is directory");
                    foreach (var file in Directory.GetFiles(localPath))
                    {
                        using (Stream fileStream = File.OpenRead(file))
                        {
                            try
                            {
                                _logger.LogInformation($"[{DateTime.UtcNow}] Uploading : {file}");
                                _sftpClient.UploadFile(fileStream, remotePath);
                                _logger.LogInformation($"[{DateTime.UtcNow}] File {file} uploaded successfully to {remotePath + file.Split('/')[0]}");
                            }
                            catch (Exception e)
                            {
                                _logger.LogError($"[{DateTime.UtcNow}] Could not upload {file} to {remotePath}");
                            }

                        }
                    }
                }
                else if (isFile)
                {
                    using (Stream fileStream = File.OpenRead(localPath))
                    {
                        try
                        {
                            _logger.LogInformation($"[{DateTime.UtcNow}] Uploading : {localPath}");
                            _sftpClient.UploadFile(fileStream, remotePath);
                            _logger.LogInformation($"[{DateTime.UtcNow}] File {localPath} uploaded successfully to {remotePath}");
                        }
                        catch (Exception e)
                        {
                            _logger.LogError($"[{DateTime.UtcNow}] Could not upload {localPath} to {remotePath}");
                        }

                    }
                }


            }
            catch (Exception e)
            {
                _logger.LogError($"[{DateTime.UtcNow}] Could not upload {localPath} to {remotePath}. Reason : {e.Message}");
            }



        }

        public override Task ConnectSSH(RemoteHost host)
        {
            try
            {
                var privateKey = host.ConnectionMethod != SSHConnectionMethod.DEFAULT ? new PrivateKeyFile(new MemoryStream(File.ReadAllBytes(_config.SSHKeysPATH + host.Name))) : null;
                _client = privateKey == null ? new SshClient(host.RemoteIP, host.SSHPort, host.SSHUsername, host.SSHPassword) : new SshClient(host.RemoteIP, host.SSHPort, host.SSHUsername, privateKey);

                _logger.LogInformation("[{time}] [SSH] Connecting to RemoteHost : {ip} ...", DateTime.UtcNow, host.RemoteIP);
                _client.Connect();
            }
            catch (FileNotFoundException e)
            {
                _logger.LogError("[{time}] [SSH] Could not find private key file for RemoteHost : {ip} ...", DateTime.UtcNow, host.RemoteIP);
                return Task.FromException(e);
            }
            catch (SshAuthenticationException e)
            {
                _logger.LogError("[{time}] [SSH] Could not find authenticate to RemoteHost : {ip} ...", DateTime.UtcNow, host.RemoteIP);
                return Task.FromException(e);
            }
            catch (SshConnectionException e)
            {
                _logger.LogError("[{time}] [SSH] Could not connect to RemoteHost : {ip} ! Check your network and try again ...", DateTime.UtcNow, host.RemoteIP);
            }
            catch (Exception e)
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
            if (_client == null || !_client.IsConnected) throw new SSHClientNotConnectedException();
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
