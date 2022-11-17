using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SteerMyWheel.Core.Connectivity.ClientProviders;
using SteerMyWheel.Configuration;
using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Domain.Discovery.CronParsing;
using SteerMyWheel.Core.Discovery.Crontab.GraphWriter;
using SteerMyWheel.Domain.Discovery.CronParsing.ReaderState;
using SteerMyWheel.Core.Model.WorkersQueue;
using SteerMyWheel.Core.Workers.Migration.Git;
using SteerMyWheel.Core.Services;
using SteerMyWheel.Core.Discovery.Crontab.Reader;
using System.Collections.Generic;
using SteerMyWheel.Domain.Model.WorkerQueue;
using SteerMyWheel.Core.Workers.Discovery;
using SteerMyWheel.Core.Connectivity.Repositories;
using System.Threading;

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
                    .AddScoped<CronGraphWriter>()
                    .AddScoped<ReaderStateContext>()
                    .AddScoped<CronReader>()
                    .AddTransient<CronParser>()
                    .AddTransient<WorkersQueue<GitMigrationWorker>>()
                    .AddTransient<WorkersQueue<CronDiscoveryWorker>>()
                    .AddTransient<ScriptSyncService>()
                    .AddSingleton<CronDiscoveryService>()
                    
                    );
            var host = builder.Build();
            var globalConfig = host.Services.GetRequiredService<GlobalConfig>();
            var remoteHosts = new List<RemoteHost>
            {
                new RemoteHost("UATFRTAPP901","UATFRTAPP901",22,"kch-front","Supervision!")
            };
            var discoveryService = host.Services.GetRequiredService<CronDiscoveryService>();
            var syncService = host.Services.GetRequiredService<ScriptSyncService>();
            var bitbucket = host.Services.GetRequiredService<BitbucketClientProvider>();
            discoveryService.setLoggerFactory(loggerFactory);
            syncService.setLoggerFactory(loggerFactory);
            foreach (var remoteHost in remoteHosts)
            {
                discoveryService.Discover(remoteHost).Wait();
                syncService.generateGraphRepos(remoteHost).Wait();
                syncService.syncRepos(remoteHost).Wait();
               
                
                
            }
            //discoveryService._queue.DeqeueAllAsync(CancellationToken.None).Wait();
            


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
                .AddTransient<CronParser>()
                .AddTransient<CronGraphWriter>();
                

        }   
    }
}
