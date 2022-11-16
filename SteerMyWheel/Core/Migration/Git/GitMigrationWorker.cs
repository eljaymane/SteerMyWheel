using Microsoft.Extensions.Logging;
using SteerMyWheel.Connectivity.ClientProviders;
using SteerMyWheel.Model;
using SteerMyWheel.TaskQueue;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SteerMyWheel.Workers.Git
{
    public class GitMigrationWorker : BaseWorker
    {
        public readonly ScriptRepository _scriptRepository;
        
        private const string gitRootUri = "https://gitlab.keplercheuvreux.com/it-front/scripts/";
        private const string gitCommandoURI = "https://gitlab.keplercheuvreux.com/it-front/commando/";
        public GitMigrationWorker(ScriptRepository scriptRepository)
        {
            _scriptRepository = scriptRepository;
        }

        public override async Task doWork()
        {
            await cloneLegacyRepo();
        }

        private async Task cloneLegacyRepo()
        {
            Logger.LogInformation("[{time}] Started cloning legacy repository {name} ..", DateTime.UtcNow, _scriptRepository.Name);
            var cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
            var createRootDir = new TransformBlock<Process, Process>( _cmd =>
            {
                return CreateRootDirectory(_cmd);
            },
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = cancellationToken
                });
            var cloneFromScripts = new TransformBlock<Process,Process>( _cmd =>
            {

                return CloneFromScripts(_cmd);

            },
            new ExecutionDataflowBlockOptions
            {

                CancellationToken = cancellationToken

            });

            var cloneFromCommando = new TransformBlock<Process,Process>(_cmd =>
            {

                return CloneFromCommando(_cmd);

            }, new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken
            });

            var closeCmd = new ActionBlock<Process>(async cmd =>
            {
                cmd.StartInfo.RedirectStandardOutput = true;
                StreamReader reader = cmd.StandardOutput;
                var result = await reader.ReadToEndAsync();
                if (result.Contains("success:true"))
                {
                   
                }
                else Logger.LogInformation("[{time}] Could not clone repository {name}", DateTime.UtcNow, _scriptRepository.Name);
                cmd.Close();
            });

            

          
            createRootDir.LinkTo(cloneFromScripts, new DataflowLinkOptions { PropagateCompletion = true});
            cloneFromScripts.LinkTo(cloneFromCommando, new DataflowLinkOptions { PropagateCompletion = true });
            cloneFromCommando.LinkTo(closeCmd, new DataflowLinkOptions { PropagateCompletion = true });
            createRootDir.Completion.ContinueWith(delegate { cloneFromScripts.Complete(); });
            cloneFromScripts.Completion.ContinueWith(delegate { cloneFromCommando.Complete(); });
            cloneFromCommando.Completion.ContinueWith(delegate { closeCmd.Complete(); });


            
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
                
                //closeCmd.Completion.Wait();
               

            }


            }

        private Process CreateRootDirectory(Process _cmd)
        {
            var cdBase = "cd C:/steer/";
            var mkdir = "mkdir steer";
            if (!Directory.Exists("C:/steer/"))
            {
                _cmd.StandardInput.WriteLine(cdBase);
                _cmd.StandardInput.FlushAsync().Wait();
                _cmd.StandardInput.WriteLine(mkdir);
                _cmd.StandardInput.FlushAsync().Wait();
            }
            return _cmd;
        }

        private Process CloneFromScripts(Process cmd)
        {
            var cd = "cd C:/steer/";
            var clone = $"git clone {gitRootUri}{_scriptRepository.Name}.git && echo success:true";
            cmd.StandardInput.WriteLine(cd);
            cmd.StandardInput.Flush();
            Logger.LogInformation("[{time}] Cloning repository {name} ..", DateTime.UtcNow, _scriptRepository.Name);
            cmd.StandardInput.WriteLine(clone);
            cmd.StandardInput.Flush();
            return cmd;

        }

        private async Task<Process> CloneFromCommando(Process cmd)
        {
           // cmd.Start();
            //var result = await cmd.StandardOutput.ReadToEndAsync();
            if (Directory.Exists(_scriptRepository.Name)) return cmd;
            else
            {
                Logger.LogInformation("[{time}] Cloning repository {name} timed out. Maybe it's in commando repo ? Trying again ...", DateTime.UtcNow, _scriptRepository.Name);
                var cd = "cd C:/steer/";
                var clone = $"git clone {gitCommandoURI}{_scriptRepository.Name}.git";
                cmd.StandardInput.WriteLine(cd);
                cmd.StandardInput.Flush();
                cmd.StandardInput.WriteLine(clone);
                Logger.LogInformation("[{time}] Cloning repository {name} ..", DateTime.UtcNow, _scriptRepository.Name);
                cmd.StandardInput.Flush();
                return cmd;
            }
        
        }

        private async Task createNewRepository()
        {
            Logger.LogInformation("[{time}] Creating repository {name} on BitBucket ..", DateTime.UtcNow, _scriptRepository.Name);
            await _BitClient.Connect();
            if (await ((BitbucketClientProvider)_BitClient).createRepositoryAsync(_scriptRepository.Name, true))
                Logger.LogInformation("[{time}] Successfully created repository {name} on BitBucket ..", DateTime.UtcNow, _scriptRepository.Name);
            else Logger.LogInformation("[{time}] Could not create repository {name} on BitBucket ..", DateTime.UtcNow, _scriptRepository.Name);

        }

        private Task downloadRecentVersion()
        {
            return Task.CompletedTask;
        }
    }
}
