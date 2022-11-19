using Microsoft.Extensions.Logging;
using SteerMyWheel.Configuration;
using SteerMyWheel.Core.Connectivity.ClientProviders;
using SteerMyWheel.Core.Connectivity.Repositories;
using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Domain.Model.WorkerQueue;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SteerMyWheel.Core.Workers.Migration.Git
{
    public class GitMigrationWorker : BaseWorker
    {
        public readonly ScriptRepository _scriptRepository;

        private const string gitRootUri = "https://gitlab.keplercheuvreux.com/it-front/scripts/";
        private const string gitCommandoURI = "https://gitlab.keplercheuvreux.com/it-front/commando/";
        private GlobalEntityRepository _DAO;
        private Process cmd;
        public GitMigrationWorker(ScriptRepository scriptRepository)
        {
            _scriptRepository = scriptRepository;
            cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
        }

        public void setGlobalEntityRepository(GlobalEntityRepository dao)
        {
            _DAO = dao;
        }

        public override async Task doWork()
        {
            await cloneLegacyRepoAsync();
        }

        private async Task syncMachineAndGit()
        {
            var syncRemoteRepo = new TransformBlock<Tuple<Process, string>, Process>(new Func<Tuple<Process, string>, Process>(cdDirectory));
            var downloadFromServer = new TransformBlock<RemoteHost, Tuple<Process,string>>(new Func<RemoteHost, Task<Tuple<Process,string>>>(downloadRecentVersion));
            var updateRemote = new TransformBlock<Process, Process>(data =>
            {
                var cmd = cdDirectory(new Tuple<Process, string>(data, _globalConfig.LocalWorkingDirectory + _scriptRepository.Name));
                cmd = removeRemote(cmd);
                return setNewRemote(cmd);
            });
            var addCommit = new TransformBlock<Process,Process>( data =>
            {
                var cmd = gitAdd(data);
                return gitCommit(cmd);
            });

            var pushAll = new TransformBlock<Process, Process>(data =>
            {
                data.StandardOutput.DiscardBufferedData();
                return gitPushAll(data, true);
            });

            var verify = new ActionBlock<Process>(data =>
            {
                if (data.StandardOutput.ReadToEnd().Contains("success:true")) _logger.LogInformation($"[{DateTime.UtcNow}] Successfully updated repository {_scriptRepository.Name} on Bitbucket !");
                else _logger.LogError($"[{DateTime.UtcNow}] Could not update reporistory {_scriptRepository.Name} on Bibucket ...");
            });
            downloadFromServer.LinkTo(syncRemoteRepo);
            syncRemoteRepo.LinkTo(updateRemote);
            updateRemote.LinkTo(addCommit);
            addCommit.LinkTo(pushAll);
            pushAll.LinkTo(verify);


        }

        private async Task cloneLegacyRepoAsync()
        {
            _logger.LogInformation("[{time}] Started cloning legacy repository {name} ..", DateTime.UtcNow, _scriptRepository.Name);
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
            var createRootDir = new TransformBlock<Process, Process>(_cmd =>
            {
                return CreateRootDirectory(_cmd);
            },
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = cancellationToken
                });
            var cloneFromScripts = new TransformBlock<Process, Process>(_cmd =>
            {

                return CloneFromScripts(_cmd);

            },
            new ExecutionDataflowBlockOptions
            {

                CancellationToken = cancellationToken

            });

            var cloneFromCommando = new TransformBlock<Process, Process>(_cmd =>
            {

                return CloneFromCommando(_cmd);

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

                }
                else _logger.LogInformation("[{time}] Could not clone repository {name}", DateTime.UtcNow, _scriptRepository.Name);
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

        private Process CreateRootDirectory(Process _cmd)
        {
            var cdBase = "cd C:/";
            var RootDirName = _globalConfig.LocalWorkingDirectory.Replace("C:/", "").Replace("/", "");
            var mkdir = $"mkdir {RootDirName}";
            if (!Directory.Exists($"{_globalConfig.LocalWorkingDirectory}"))
            {
                _cmd.StandardInput.WriteLine(cdBase);
                _cmd.StandardInput.FlushAsync().Wait();
                _cmd.StandardInput.WriteLine(mkdir);
                _cmd.StandardInput.FlushAsync().Wait();
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

        private Process CloneFromScripts(Process cmd)
        {
            cmd = cdDirectory(new Tuple<Process,string>(cmd,_globalConfig.LocalWorkingDirectory));
            var clone = $"git clone {gitRootUri}{_scriptRepository.Name}.git && echo success:true";
            _logger.LogInformation("[{time}] Cloning repository {name} ..", DateTime.UtcNow, _scriptRepository.Name);
            cmd.StandardInput.WriteLine(clone);
            cmd.StandardInput.Flush();
            return cmd;

        }

        private Task<Process> CloneFromCommando(Process cmd)
        {
            if (Directory.Exists(_scriptRepository.Name)) return Task.FromResult(cmd);
            else
            {
                cmd = cdDirectory(new Tuple<Process,string>(cmd,_globalConfig.LocalWorkingDirectory));
                _logger.LogInformation("[{time}] Cloning repository {name} timed out. Maybe it's in commando repo ? Trying again ...", DateTime.UtcNow, _scriptRepository.Name);
                var clone = $"git clone {gitCommandoURI}{_scriptRepository.Name}.git && echo success:true";
                cmd.StandardInput.WriteLine(clone);
                _logger.LogInformation("[{time}] Cloning repository {name} ..", DateTime.UtcNow, _scriptRepository.Name);
                _scriptRepository.LegacyRepository = gitCommandoURI + _scriptRepository.Name;
                cmd.StandardInput.Flush();
                if(cmd.StandardOutput.ReadToEnd().Contains("success:true")) _DAO.ScriptRepositoryRepository.Update(_scriptRepository);
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

        private Process removeRemote(Process cmd)
        {
            cdDirectory(new Tuple<Process,string>(cmd,_globalConfig.LocalWorkingDirectory+_scriptRepository.Name));
            var removeRemote = "git remote rm origin";
            cmd.StandardInput.WriteLine(removeRemote);
            cmd.StandardInput.Flush();
            return cmd;
        }

        private Process setNewRemote(Process cmd)
        {
            var setNewOrigin = $"git remote set-url origin {_scriptRepository.BitbucketRepository}";
            cmd.StandardInput.WriteLine(setNewOrigin);
            cmd.StandardInput.Flush();
            return cmd;

        }

        private Process gitPushAll(Process cmd,bool SSLVerify)
        {
            var pushAll = SSLVerify ? "git -c http.sslVerify=false push --all && echo success:true" : "git push --all && echo success:true";
            cmd.StandardInput.Write(pushAll);
            cmd.StandardInput.Flush();
            return cmd;

        }

        private async Task<Tuple<Process,string>> downloadRecentVersion(RemoteHost host)
        {
            var scriptExecutions = _DAO.ScriptExecutionRepository.GetAll(_scriptRepository);
            var name = scriptExecutions.First().Name;
            if (ParserConfig.isJava(name))
            {
                _logger.LogInformation($"[{DateTime.UtcNow}] Java repository detected ({_scriptRepository.Name}, Skipping..");
                await Task.FromCanceled(CancellationToken.None);

            } else if(ParserConfig.isPerl(name) || ParserConfig.isPython(name) || ParserConfig.isBash(name))
            {
                var type = ParserConfig.isPython(name) ? "python" : ParserConfig.isPerl(name) ? "perl" : ParserConfig.isBash(name) ? "bash" : "unknown";
                _logger.LogInformation($"[{DateTime.UtcNow}] {type} repository detected for {_scriptRepository.Name}, starting to download files ...");
                await _sshClient.ConnectSFTP(_DAO.RemoteHostRepository.Get(_scriptRepository));
                await _sshClient.DownloadRepository(_scriptRepository, _globalConfig.LocalWorkingDirectory + "repos/" + _scriptRepository.Name);

            } else 
            {
                _logger.LogInformation($"[{DateTime.UtcNow}] Could not detect type of code for repository ({_scriptRepository.Name}, Skipping..");
                await Task.FromCanceled(CancellationToken.None);
            }
            return new Tuple<Process,string>(cmd,_globalConfig.LocalWorkingDirectory + _scriptRepository.Name);
        }

        private Process gitAdd(Process cmd)
        {
            var add = "git add .";
            cmd.StandardInput.Write(add);
            cmd.StandardInput.Flush();
            return cmd;
        }

        private Process gitCommit(Process cmd)
        {
            var commit = $"git commit -m {_globalConfig.DefaultCommitMessage}";
            cmd.StandardInput.Write(commit);
            cmd.StandardInput.Flush();
            return cmd;
        }
    }
}
