using Renci.SshNet;
using SteerMyWheel.Core.Model.Entities;
using System;
using System.IO;
using System.Threading.Tasks.Dataflow;

namespace SteerMyWheel.Core.Synchronization.Migration.SSH
{
    public class SSHMigration
    {
        public SSHMigration()
        {
        }

        public JoinBlock<ScriptExecution, RemoteHost, RemoteHost> doSSHMigration(JoinBlock<ScriptExecution, RemoteHost, RemoteHost> joinScriptSourceTarget)
        {
            var _script = joinScriptSourceTarget.Receive().Item1;
            var _source = joinScriptSourceTarget.Receive().Item2;
            var _target = joinScriptSourceTarget.Receive().Item3;
            var migrationBlock = new TransformBlock<JoinBlock<ScriptExecution, RemoteHost, RemoteHost>, JoinBlock<ScriptExecution, string, RemoteHost>>(new Func<JoinBlock<ScriptExecution, RemoteHost, RemoteHost>, JoinBlock<ScriptExecution, string, RemoteHost>>(downloadScript));
            var _uploadScript = new ActionBlock<JoinBlock<ScriptExecution, string, RemoteHost>>(data =>
            {
                uploadScript(data);
            });
            var deleteTmp = new ActionBlock<string>(data =>
            {
                DeleteTmpDir(data);
            });
            migrationBlock.LinkTo(_uploadScript);
            _uploadScript.Completion.ContinueWith(delegate { deleteTmp.Complete(); });
            migrationBlock.Completion.ContinueWith(delegate { _uploadScript.Complete(); });
            migrationBlock.Post(joinScriptSourceTarget);
            migrationBlock.Complete();
            _uploadScript.Completion.Wait();
            deleteTmp.Completion.Wait();
            return joinScriptSourceTarget;
        }

        private JoinBlock<ScriptExecution, string, RemoteHost> downloadScript(JoinBlock<ScriptExecution, RemoteHost, RemoteHost> joinScriptSourceTarget)
        {
            var script = joinScriptSourceTarget.Receive().Item1;
            var source = joinScriptSourceTarget.Receive().Item2;
            var target = joinScriptSourceTarget.Receive().Item3;
            var values = script.Path.Split('/');
            var joinScriptPathHost = new JoinBlock<ScriptExecution, string, RemoteHost>();
            var _localPath = new DirectoryInfo(Environment.CurrentDirectory + "/tmp/" + values[values.Length - 1]);
            try
            {
                using (var _client = new ScpClient(source.RemoteIP, source.SSHPort, source.SSHUsername, source.SSHPassword))
                {
                    _client.Connect();
                    _client.Download(script.Path, _localPath);
                    joinScriptPathHost.Target1.Post(script);
                    joinScriptPathHost.Target2.Post(_localPath.ToString());
                    joinScriptPathHost.Target3.Post(target);
                    return joinScriptPathHost;
                }

            }
            catch (Exception e)
            {
                return null;
            }
        }

        private string uploadScript(JoinBlock<ScriptExecution, string, RemoteHost> joinScriptPathHost)
        {
            try
            {
                var host = joinScriptPathHost.Receive().Item3;
                var localPath = joinScriptPathHost.Receive().Item2;
                var script = joinScriptPathHost.Receive().Item1;
                using (var _client = new ScpClient(host.RemoteIP, host.SSHPort, host.SSHUsername, host.SSHPassword))
                {
                    _client.Connect();
                    _client.Upload(new DirectoryInfo(localPath), script.Path);
                    return script.Path;
                }
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }
        private void DeleteTmpDir(string path)
        {
            Directory.Delete(path);
        }
    }


}
