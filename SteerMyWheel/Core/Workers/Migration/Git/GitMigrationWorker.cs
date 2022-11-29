using Microsoft.Extensions.Logging;
using SteerMyWheel.Configuration;
using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Core.Model.WorkersQueue;
using SteerMyWheel.Infrastracture.Connectivity.ClientProviders;
using SteerMyWheel.Infrastracture.Connectivity.Git;
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
            if(Directory.Exists(_globalConfig.LocalReposDirectory + _scriptRepository.Name))await syncMachineAndGit((RemoteHost)host);
            else { _logger.LogError($"[{DateTime.UtcNow}] Repository {_scriptRepository.Name} does not exist locally !"); }
        }

        private async Task syncMachineAndGit(RemoteHost host)
        {
            var path = _globalConfig.LocalReposDirectory + _scriptRepository.Name;
            var downloadFromServer = new TransformBlock<RemoteHost, string>(host => {
                downloadRecentVersion(host);
                return path;
                });
            var rmRemote = new TransformBlock<string,string>(_path =>
            {
                GitCommands.RemoveRemote(_path, _globalConfig.DefaultRemoteName);
                return _path;
            });
            var updateRemote = new TransformBlock<string,string>(_path =>
            {
                var remote = _scriptRepository.BitbucketRepository;
                GitCommands.AddRemote(_path,_globalConfig.DefaultRemoteName,remote);
                return _path;
            });
            var add = new TransformBlock<string,string>(_path =>
            {
                GitCommands.AddAllFiles(_path);
                return _path;
            });
            var commit = new TransformBlock<string, string>(_path =>
            {
                GitCommands.CreateCommit(_path, _globalConfig.DefaultCommitMessage);
                return _path;
            });
            var pushAll = new ActionBlock<string>(_path =>
            {
                GitCommands.PushAll(_path,false);
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

            downloadFromServer.Post(host);
            downloadFromServer.Complete();

            Task.WaitAll(
                downloadFromServer.Completion,
                rmRemote.Completion,
                updateRemote.Completion,
                commit.Completion
                );
            
        }

        private async Task cloneLegacyRepoAsync()
        {
            _logger.LogInformation("[{time}] Started cloning legacy repository {name} ..", DateTime.UtcNow, _scriptRepository.Name);
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
            
            var cloneFromScripts = new TransformBlock<string, string>(_path =>
            {
                GitCommands.Clone(_path, _scriptRepository.LegacyRepository, false);
                return _path;
            },
            new ExecutionDataflowBlockOptions
            {

                CancellationToken = cancellationToken

            });

            var cloneFromCommando = new TransformBlock<string, string>(_path =>
            {
                var remote = _globalConfig.gitLabCommandoBaseURI + _scriptRepository.Name;
                GitCommands.Clone(_path,remote,false);
                return _path;

            }, new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken
            });

            cloneFromScripts.LinkTo(cloneFromCommando, new DataflowLinkOptions { PropagateCompletion = true });
            cloneFromScripts.Completion.ContinueWith(delegate { cloneFromCommando.Complete(); });

            cloneFromScripts.Post(path);
                Task.WaitAll
                    (
                    cloneFromScripts.Completion,
                    cloneFromCommando.Completion
                    );
                await createNewRepository();


        }

        private async Task createNewRepository()
        {
            _logger.LogInformation("[{time}] Creating repository {name} on BitBucket ..", DateTime.UtcNow, _scriptRepository.Name);
            await _BitClient.Connect();
            if (await ((BitbucketClientProvider)_BitClient).createRepositoryAsync(_scriptRepository.Name, true))
                _logger.LogInformation("[{time}] Successfully created repository {name} on BitBucket ..", DateTime.UtcNow, _scriptRepository.Name);
            else _logger.LogInformation("[{time}] Could not create repository {name} on BitBucket ..", DateTime.UtcNow, _scriptRepository.Name);

        }

        private async Task<Process> downloadRecentVersion(RemoteHost host)
        {
            var scriptExecutions = _DAO.ScriptExecutionRepository.GetAll(_scriptRepository);
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

    }
}
