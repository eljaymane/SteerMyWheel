using Microsoft.Extensions.Logging;
using SteerMyWheel.Configuration;
using SteerMyWheel.Core.Connectivity.ClientProviders;
using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Domain.Model.WorkerQueue;
using SteerMyWheel.Infrastracture.Connectivity.ClientProviders;
using SteerMyWheel.Infrastracture.Connectivity.Repositories;
using SteerMyWheel.Misc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SteerMyWheel.Core.Workers.Migration.Git
{
    public class GitMigrationWorker : BaseWorker
    {
        public readonly ScriptRepository _scriptRepository;

        private const string gitRootUri = "https://gitlab.keplercheuvreux.com/it-front/scripts/";
        private const string gitCommandoURI = "https://gitlab.keplercheuvreux.com/it-front/commando/";
        private GlobalEntityRepository _DAO;
        private string path;
        private Process cmd;
        public GitMigrationWorker(ScriptRepository scriptRepository)
        {
            _scriptRepository = scriptRepository;
            cmd = new Process();
            
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = false;
        }

        public void setGlobalEntityRepository(GlobalEntityRepository dao)
        {
            _DAO = dao;
        }

        public void setGlobalConfig(GlobalConfig config)
        {
            base._globalConfig= config;
            path = _globalConfig.LocalReposDirectory + _scriptRepository.Name;
        }

        public void setGlobalRepository(GlobalEntityRepository DAO)
        {
            _DAO= DAO;
        }

        public void SetSSHClient(SSHClientProvider sshClient)
        {
            _sshClient= sshClient;
        }
        public override async Task doWork()
        {
            //await cloneLegacyRepoAsync();
            var host = _DAO.RemoteHostRepository.Get(_scriptRepository);
            if(Directory.Exists(_globalConfig.LocalReposDirectory + _scriptRepository.Name))await syncMachineAndGit(host);
            else { _logger.LogError($"[{DateTime.UtcNow}] Repository {_scriptRepository.Name} does not exist locally !"); }
        }

        private async Task syncMachineAndGit(RemoteHost host)
        {
            
            var syncRemoteRepo = new TransformBlock<Tuple<Process, string>, Process>(new Func<Tuple<Process, string>, Process>(cdDirectory));
            var downloadFromServer = new TransformBlock<RemoteHost,Process>(new Func<RemoteHost, Task<Process>>(downloadRecentVersion));
            var rmRemote = new TransformBlock<Process, Process>(cmd =>
            {
                return removeRemote(ref cmd);
            });
            var updateRemote = new TransformBlock<Process, Process>(data =>
            {
                return setNewRemote(ref data);
            });
            var add = new TransformBlock<Process,Process>( cmd =>
            {
                return gitAdd(ref cmd);
            });
            var commit = new TransformBlock<Process, Process>(new Func<Process, Process>(cmd =>
            {
                return gitCommit(ref cmd);
            }));
            var pushAll = new TransformBlock<Process, Process>(cmd =>
            {
                return gitPushAll(ref cmd);
            });

            var verify = new ActionBlock<Process>(async data =>
            {
                data.StartInfo.RedirectStandardOutput = true;

                StreamReader reader = data.StandardOutput;
                var result = await reader.ReadToEndAsync();
                if ( result.Contains("success:true")) _logger.LogInformation($"[{DateTime.UtcNow}] Successfully updated repository {_scriptRepository.Name} on Bitbucket !");
                else _logger.LogError($"[{DateTime.UtcNow}] Could not update reporistory {_scriptRepository.Name} on Bibucket ...");
            });
            downloadFromServer.LinkTo(rmRemote);
            rmRemote.LinkTo(updateRemote);
            updateRemote.LinkTo(add);
            add.LinkTo(commit);
            commit.LinkTo(pushAll);
            downloadFromServer.Completion.ContinueWith(delegate { rmRemote.Complete(); });
            rmRemote.Completion.ContinueWith(delegate { updateRemote.Complete(); });
            updateRemote.Completion.ContinueWith(delegate { add.Complete(); });
            add.Completion.ContinueWith(delegate { commit.Complete(); });
            commit.Completion.ContinueWith(delegate { pushAll.Complete(); });


            if (cmd.Start())
            {
                downloadFromServer.Post(host);
                downloadFromServer.Complete();
                Task.WaitAll(
                    downloadFromServer.Completion,
                    rmRemote.Completion,
                    updateRemote.Completion,
                    commit.Completion
                    );
            }
            
        }

        private async Task cloneLegacyRepoAsync()
        {
            _logger.LogInformation("[{time}] Started cloning legacy repository {name} ..", DateTime.UtcNow, _scriptRepository.Name);
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
            var createRootDir = new TransformBlock<Process, Process>(_cmd =>
            {
                return CreateRootDirectory(ref _cmd);
            },
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = cancellationToken
                });
            var cloneFromScripts = new TransformBlock<Process, Process>(_cmd =>
            {

                return CloneFromScripts(ref _cmd);

            },
            new ExecutionDataflowBlockOptions
            {

                CancellationToken = cancellationToken

            });

            var cloneFromCommando = new TransformBlock<Process, Process>(_cmd =>
            {

                return CloneFromCommando(ref _cmd);

            }, new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken
            });

            var verify = new ActionBlock<Process>(async cmd =>
            {
                cmd.StartInfo.RedirectStandardOutput = true;
                StreamReader reader = cmd.StandardOutput;
                var result = await reader.ReadToEndAsync();
                if (result.Contains("success:true"))
                {
                    _logger.LogInformation($"[{DateTime.UtcNow}] Successfully cloned repository {_scriptRepository.Name} !");
                }
                else _logger.LogError("[{time}] Could not clone repository {name}", DateTime.UtcNow, _scriptRepository.Name);
                cmd.Close();
            });




            createRootDir.LinkTo(cloneFromScripts, new DataflowLinkOptions { PropagateCompletion = true });
            cloneFromScripts.LinkTo(cloneFromCommando, new DataflowLinkOptions { PropagateCompletion = true });
            cloneFromCommando.LinkTo(verify, new DataflowLinkOptions { PropagateCompletion = true });
            createRootDir.Completion.ContinueWith(delegate { cloneFromScripts.Complete(); });
            cloneFromScripts.Completion.ContinueWith(delegate { cloneFromCommando.Complete(); });
            cloneFromCommando.Completion.ContinueWith(delegate { verify.Complete(); });



            if (cmd.Start())
            {

                createRootDir.Post(cmd);
                createRootDir.Complete();
                Task.WaitAll
                    (
                    createRootDir.Completion,
                    cloneFromScripts.Completion,
                    cloneFromCommando.Completion
                    );
                await createNewRepository();

            }


        }

        private Process CreateRootDirectory(ref Process _cmd)
        {
            var cdBase = "cd C:/";
            var RootDirName = _globalConfig.LocalWorkingDirectory.Replace("C:/", "").Replace("/", "");
            var mkdir = $"mkdir {RootDirName}";
            if (!Directory.Exists($"{_globalConfig.LocalWorkingDirectory}"))
            {
                _cmd.StandardInput.WriteLine(cdBase);
                _cmd.StandardInput.Flush();
                _cmd.StandardInput.WriteLine(mkdir);
                _cmd.StandardInput.Flush();
            }
            return _cmd;
        }

        private Process cdDirectory(Tuple<Process,string> cmdAndPath)
        {
            
            var cmd = cmdAndPath.Item1;
            var directory = cmdAndPath.Item2;
            var cd = $"cd {directory}";
            cmd.StandardInput.Write(cd);
            cmd.StandardInput.Flush();
            return cmd;
        }

        private Process CloneFromScripts(ref Process cmd)
        {
            cmd = cdDirectory(new Tuple<Process,string>(cmd,_globalConfig.LocalWorkingDirectory));
            var clone = $"git clone {gitRootUri}{_scriptRepository.Name}.git && echo success:true";
            _logger.LogInformation("[{time}] [PID : {ID}] Cloning repository {name} ..", DateTime.UtcNow,cmd.Id, _scriptRepository.Name);
            cmd.StandardInput.WriteLine(clone);
            cmd.StandardInput.FlushAsync().Wait();
            return cmd;

        }

        private Task<Process> CloneFromCommando(ref Process cmd)
        {
            if (Directory.Exists(_scriptRepository.Name)) return Task.FromResult(cmd);
            else
            {
                cmd.Start();
                cmd = cdDirectory(new Tuple<Process,string>(cmd,_globalConfig.LocalWorkingDirectory));
                _logger.LogInformation("[{time}] [PID : {ID}] Cloning repository {name} timed out. Maybe it's in commando repo ? Trying again ...", DateTime.UtcNow,cmd.Id, _scriptRepository.Name);
                var clone = $"git clone {gitCommandoURI}{_scriptRepository.Name}.git && echo success:true";
                cmd.StandardInput.WriteLine(clone);
                _logger.LogInformation("[{time}] Cloning repository {name} ..", DateTime.UtcNow, _scriptRepository.Name);
                _scriptRepository.LegacyRepository = gitCommandoURI + _scriptRepository.Name;
                cmd.StandardInput.Flush();
                if (Directory.Exists(_scriptRepository.Name)) _DAO.ScriptRepositoryRepository.Update(_scriptRepository);
                return Task.FromResult(cmd);
            }

        }

        private async Task createNewRepository()
        {
            _logger.LogInformation("[{time}] Creating repository {name} on BitBucket ..", DateTime.UtcNow, _scriptRepository.Name);
            await _BitClient.Connect();
            if (await ((BitbucketClientProvider)_BitClient).createRepositoryAsync(_scriptRepository.Name, true))
                _logger.LogInformation("[{time}] Successfully created repository {name} on BitBucket ..", DateTime.UtcNow, _scriptRepository.Name);
            else _logger.LogInformation("[{time}] Could not create repository {name} on BitBucket ..", DateTime.UtcNow, _scriptRepository.Name);

        }

        private Process removeRemote(ref Process cmd)
        {
            WinAPI.system($"cd {path} && git remote rm origin");
            _logger.LogInformation($"[{DateTime.UtcNow}] Removed remote origin from repository {_scriptRepository.Name} !");
            return cmd;
        }

        private Process setNewRemote(ref Process cmd)
        {
            
            var setNewOrigin = $"cd {path} && git remote add origin {_scriptRepository.BitbucketRepository}";
            WinAPI.system(setNewOrigin);
            _logger.LogInformation($"[{DateTime.UtcNow}] Setted up new origin for repository {_scriptRepository.Name} !");
            return cmd;

        }

        private Process gitPushAll(ref Process cmd)
        {
            var SSLVerify = true;
            
            var pushAll = SSLVerify ? $"cd {path} && git -c http.sslVerify=false push --all && echo success:true" : $"cd {path} && git push --all && echo success:true";
            WinAPI.system(pushAll);
            _logger.LogInformation($"[{DateTime.UtcNow}] Successfully pushed files of repository {_scriptRepository.Name} !");
            return cmd;

        }

        private async Task<Process> downloadRecentVersion(RemoteHost host)
        {
            IEnumerable s;
            var scriptExecutions = _DAO.ScriptExecutionRepository.GetAll(_scriptRepository);
            //scriptExecutions = from script in scriptExecutions where script.Name.Trim() != "" select script;
            var name = scriptExecutions.First().Name;
            if (name.Trim() == "") return null;
            if (ParserConfig.isJava(name))
            {
                _logger.LogInformation($"[{DateTime.UtcNow}] Java repository detected ({_scriptRepository.Name}, Skipping..");
                await Task.FromCanceled(CancellationToken.None);

            } else if(ParserConfig.isPerl(name) || ParserConfig.isPython(name) || ParserConfig.isBash(name))
            {
                var type = ParserConfig.isPython(name) ? "python" : ParserConfig.isPerl(name) ? "perl" : ParserConfig.isBash(name) ? "bash" : "unknown";
                _logger.LogInformation($"[{DateTime.UtcNow}] {type} repository detected for {_scriptRepository.Name}, starting to download files ...");
                await _sshClient.ConnectSFTP(_DAO.RemoteHostRepository.Get(_scriptRepository));
                await _sshClient.DownloadDirectory(_scriptRepository.Path, _globalConfig.LocalWorkingDirectory + "repos/" + _scriptRepository.Name);

            } else 
            {
                _logger.LogInformation($"[{DateTime.UtcNow}] Could not detect type of code for repository ({_scriptRepository.Name}, Skipping..");
                await Task.FromCanceled(CancellationToken.None);
            }
            cmd.Start();
            return cmd;
        }

        private Process gitAdd(ref Process cmd)
        {
            var add = $"cd {path} && git add .";
            WinAPI.system(add);
            _logger.LogInformation($"[{DateTime.UtcNow}] Tracked all files of repository {_scriptRepository.Name} !");
            return cmd;
        }

        private Process gitCommit(ref Process cmd)
        {
            cmd = cdDirectory(new Tuple<Process, string>(cmd, path));
            var commit = $"git commit -m \"{_globalConfig.DefaultCommitMessage}\"";
            cmd.StandardInput.Write(commit);
            cmd.StandardInput.FlushAsync().Wait();
            _logger.LogInformation($"[{DateTime.UtcNow}] Created commit for tracked files of repository {_scriptRepository.Name} !");
            return cmd;
        }
    }
}
