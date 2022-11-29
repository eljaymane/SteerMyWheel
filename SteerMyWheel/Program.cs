using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SteerMyWheel.Configuration;
using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Core.Model.WorkersQueue;
using SteerMyWheel.Core.Workers.Migration.Git;
using SteerMyWheel.Core.Services;
using System.Collections.Generic;
using SteerMyWheel.Core.Workers.Discovery;
using System.Threading;
using SteerMyWheel.Infrastracture.Connectivity.Repositories;
using SteerMyWheel.Infrastracture.Connectivity.ClientProviders;
using SteerMyWheel.Core.Model.CronReading;
using SteerMyWheel.Core.Model.Workflows;
using SteerMyWheel.Core.Model.Workflows.CommandExecution;
using System;
using SteerMyWheel.Core.Model.Workflows.Monitoring;

namespace SteerMyWheel
{
    internal class Program
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
                    .AddTransient<SSHClientProvider>()
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

            var workflowService = host.Services.GetRequiredService<WorkflowsThreadsQueue>();
            var context = new WorkflowStateContext(loggerFactory);
            var executionDate = DateTime.Now;
            executionDate = executionDate.AddMinutes(1);
            var workflow = new MonitorFilesWorkflow(new string[] { "c:/test/test.txt" }, "test", "test", executionDate, null, null);
            var workflow2 = new LocalCommandExecution("echo`\"File has been found, 2nd execution done.\"", "test","test",executionDate.AddMinutes(1), null, null);
            workflow2.Previous = workflow;
            workflow.Next = workflow2;
            context.Initialize(workflow, CancellationToken.None);
            var workflowThread = new WorkflowThread(loggerFactory.CreateLogger<WorkflowThread>(),context);
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
