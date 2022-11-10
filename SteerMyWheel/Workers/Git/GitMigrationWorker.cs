using Microsoft.Extensions.Logging;
using SteerMyWheel.CronParsing.Model;
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
        private readonly Script _script;
        private const string gitRootUri = "https://gitlab.keplercheuvreux.com/it-front/scripts/";
        public GitMigrationWorker(Script script)
        {
            _script = script;
        }
        public override async Task doWork()
        {
            await cloneLegacyRepo();
        }

        private async Task cloneLegacyRepo()
        {
            Logger.LogInformation("[{time}] Started cloning legacy repository {name} ..", DateTime.UtcNow, _script.name);
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
                var clone = $"git clone {gitRootUri}{_script.name}.git";
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
                Logger.LogInformation("[{time}] Cloning repository {name} ..", DateTime.UtcNow, _script.name);
                cmd.StandardInput.WriteLine(clone);
                await cmd.StandardInput.FlushAsync();
                cmd.Close();
                cmd.WaitForExit();
                Logger.LogInformation("[{time}] Repository {name} successfully cloned ..", DateTime.UtcNow, _script.name);
            }
            

        }

        private Task downloadRecentVersion()
        {
            return Task.CompletedTask;
        }
    }
}
