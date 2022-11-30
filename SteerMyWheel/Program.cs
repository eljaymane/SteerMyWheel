using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SteerMyWheel.Configuration;
using SteerMyWheel.Core.Model.CronReading;
using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Core.Model.WorkersQueue;
using SteerMyWheel.Core.Model.Workflows;
using SteerMyWheel.Core.Model.Workflows.CommandExecution;
using SteerMyWheel.Core.Model.Workflows.Factory;
using SteerMyWheel.Core.Model.Workflows.Monitoring;
using SteerMyWheel.Core.Services;
using SteerMyWheel.Core.Workers.Discovery;
using SteerMyWheel.Core.Workers.Migration.Git;
using SteerMyWheel.Infrastracture.Connectivity;
using SteerMyWheel.Infrastracture.Connectivity.ClientProviders;
using SteerMyWheel.Infrastracture.Connectivity.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SteerMyWheel
{
    public class Program
    {
        //args[0] => cron file
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                    .AddConsole();
            });

            var builder = CreateHostBuilder(args).ConfigureServices((_, services) => services
            .AddLogging(configure =>
            configure.AddConsole()
            )
                    .AddTransient<GlobalConfig>()
                    .AddTransient<NeoClientProvider>()
                    .AddTransient<BitbucketClientProvider>()
                    .AddTransient<SSHClient>()
                    .AddTransient<ScriptExecutionRepository>()
                    .AddTransient<ScriptRepositoryRepository>()
                    .AddTransient<RemoteHostRepository>()
                    .AddTransient<GlobalEntityRepository>()
                    .AddScoped<ReaderStateContext>()
                    .AddScoped<CronReader>()
                    .AddTransient<CronParser>()
                    .AddTransient<WorkersQueue<GitMigrationWorker>>()
                    .AddTransient<WorkersQueue<CronDiscoveryWorker>>()
                    .AddTransient<ScriptSyncService>()
                    .AddSingleton<CronDiscoveryService>()
                    .AddSingleton<WorkflowsThreadsQueue>()
                    .AddSingleton<WorkflowContextFactory>()
                    .AddSingleton<SSHClientFactory>()
                    );
            var host = builder.Build();
            var globalConfig = host.Services.GetRequiredService<GlobalConfig>();
            var remoteHosts = new List<RemoteHost>
            {
                new RemoteHost("PRDFRTAPP901","PRDFRTAPP901",22,"kch-front","Supervision!")
            };
            //var discoveryService = host.Services.GetRequiredService<CronDiscoveryService>();
            //var syncService = host.Services.GetRequiredService<ScriptSyncService>();
            //var bitbucket = host.Services.GetRequiredService<BitbucketClientProvider>();
            //discoveryService.setLoggerFactory(loggerFactory);
            //syncService.setLoggerFactory(loggerFactory);
            //foreach (var remoteHost in remoteHosts)
            //{
            //discoveryService.Discover(remoteHost).Wait();
            //syncService.generateGraphRepos(remoteHost).Wait();
            //syncService.syncRepos(remoteHost).Wait();




            //}
            //discoveryService._queue.DeqeueAllAsync(CancellationToken.None).Wait();
            var sshFactory = host.Services.GetRequiredService<SSHClientFactory>();
            var workflowService = host.Services.GetRequiredService<WorkflowsThreadsQueue>();
            var context = new WorkflowStateContext(loggerFactory.CreateLogger<BaseWorkflowContext>(), "Wait and execute");
            var executionDate = DateTime.Now;
            executionDate = executionDate.AddMinutes(1);
            var workflow = new MonitorLocalFilesWorkflow(new string[] { "c:/test/test.txt" }, "test", "test", executionDate, null, null);
            var workflow2 = new LocalCommandExecutionWorkflow("echo File has been found, 2nd execution done.", "test", "test", executionDate.AddMinutes(1), null, null);
            var workflow3 = new MonitorFilesSSHWorkflow(sshFactory.CreateSSHClient(), remoteHosts.First(), new string[] { "/home/kch-front/scripts/ul_iris/bin/irisDailyConfig" }, "file downbload iris", "test", executionDate.AddMinutes(1), null, null);
            var workflowFactory = host.Services.GetRequiredService<WorkflowContextFactory>();
            Queue<BaseWorkflow> q = new Queue<BaseWorkflow>();
            q.Enqueue(workflow);
            q.Enqueue(workflow3);
            q.Enqueue(workflow2);
            //context.Initialize(workflow, CancellationToken.None);
            var workflowThread = new WorkflowThread(loggerFactory.CreateLogger<WorkflowThread>(), workflowFactory.CreateContext(q, "Wait and execute"));
            workflowService.Enqueue(workflowThread);
            workflowService.Process();
            Console.ReadKey();
            host.RunAsync().Wait();




        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                 .ConfigureAppConfiguration(configuration =>
                 {
                     configuration.AddUserSecrets<GlobalConfig>();
                 });
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddConsole())
                .AddTransient<ReaderStateContext>()
                .AddTransient<CronParser>();


        }
    }
}
