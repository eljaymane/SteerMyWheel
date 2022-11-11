using Microsoft.Extensions.Logging;
using SteerMyWheel.CronParsing.Model;
using SteerMyWheel.Discovery.Model;
using SteerMyWheel.TaskQueue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Workers.Git
{
    public class GitMigrationWorker : BaseWorker
    {
        private readonly ScriptRepository _scriptRepository;
        private readonly GlobalConfig _globalConfig;
        private const string gitRootUri = "https://gitlab.keplercheuvreux.com/it-front/scripts/";
        public GitMigrationWorker(ScriptRepository scriptRepository,GlobalConfig globalConfig)
        {
            _scriptRepository = scriptRepository;
            _globalConfig = globalConfig;
        }
        public override async Task doWork()
        {
            await cloneLegacyRepo();
        }

        private async Task cloneLegacyRepo()
        {
            Logger.LogInformation("[{time}] Started cloning legacy repository {name} ..", DateTime.UtcNow, _scriptRepository.name);
            var cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            //cmd.StandardInput.AutoFlush = true;
            if (cmd.Start())
            {
                var cdBase = "cd C:/";
                var mkdir = "mkdir steer";
                var cd = "cd C:/steer/";
                var clone = $"git clone {gitRootUri}{_scriptRepository.name}.git";
                Logger.LogInformation("[{time}] Creating directory  ..", DateTime.UtcNow);
                if (!Directory.Exists("C:/steer/"))
                {
                    cmd.StandardInput.WriteLine(cdBase);
                    await cmd.StandardInput.FlushAsync();
                    cmd.StandardInput.WriteLine(mkdir);
                    await cmd.StandardInput.FlushAsync();
                    
                }
                cmd.StandardInput.WriteLine(cd);
                await cmd.StandardInput.FlushAsync();
                Logger.LogInformation("[{time}] Cloning repository {name} ..", DateTime.UtcNow, _scriptRepository.name);
                cmd.StandardInput.WriteLine(clone);
                await cmd.StandardInput.FlushAsync();
                cmd.Close();
                cmd.WaitForExit();
                Logger.LogInformation("[{time}] Repository {name} successfully cloned ..", DateTime.UtcNow, _scriptRepository.name);
            }
            

            }
            
        private async Task createNewRepository()
        {
            Logger.LogInformation("[{time}] Started cloning legacy repository {name} ..", DateTime.UtcNow, _scriptRepository.name);
            var cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            var createRepo = $"curl --user {_globalConfig.bitbucketUsername}:{_globalConfig.bitbucketPassword} https://api.bitbucket.org/2.0/repositories \\ --data name={_scriptRepository.name}";
            if (cmd.Start())
            {
                cmd.StandardInput.WriteLineAsync();
            }

        }

        private Task downloadRecentVersion()
        {
            return Task.CompletedTask;
        }
    }
}
