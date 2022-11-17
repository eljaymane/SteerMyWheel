using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SteerMyWheel.Workers;
using SteerMyWheel.Core.Connectivity.ClientProviders;
using SteerMyWheel.Configuration;
using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Domain.Discovery.CronParsing;
using SteerMyWheel.Core.Discovery.Crontab.GraphWriter;
using SteerMyWheel.Domain.Discovery.CronParsing.ReaderState;
using SteerMyWheel.Core.Discovery.Crontab.CronReader;
using SteerMyWheel.Core.Synchronization;
using SteerMyWheel.Core.Synchronization.Migration.Git;
using SteerMyWheel.Core.Model.WorkersQueue;

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
                    .AddTransient<CronGraphWriter>()
                    .AddTransient<ReaderStateContext>()
                    .AddTransient<CronReader>()
                    .AddTransient<CronParser>()
                    .AddTransient<WorkersQueue<GitMigrationWorker>>()
                    .AddTransient<ScriptSyncService>());
            var host = builder.Build();
            var globalConfig = host.Services.GetRequiredService<GlobalConfig>();
            //var _reader = host.Services.GetRequiredService<CronReader>();
            //_reader.GetContext().Initialize(new RemoteHost("PRDFRTAPP901", "PRDFRTAPP901", 22, "kcm-front", "Supervision!"));
            //_reader.Read("C:/scripts.txt").Wait();
            var syncService = host.Services.GetRequiredService<ScriptSyncService>();
            syncService.setLoggerFactory(loggerFactory);
            var h = new RemoteHost("PRDFRTAPP901", "PRDFRTAPP901", 22, "kcm-front", "Supervision!");
            //syncService.generateGraphRepos(h).Wait();
            syncService.syncRepos(h).Wait();

            //syncService.generateGraphRepos(h);
            //var bitbucket = host.Services.GetRequiredService<BitbucketClient>();
            //bitbucket.getAccessCode();
            //bitbucket.Connect().Wait();
            //bitbucket.createRepository("test",true);
            //bitbucket.createRepository("test");

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
                .AddTransient<CronGraphWriter>()
                .AddTransient<TestWorker>();
                

        }   
    }
}
