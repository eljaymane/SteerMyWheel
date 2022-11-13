using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration.UserSecrets;
using Neo4jClient.Cypher;
using SteerMyWheel.CronParsing;
using SteerMyWheel.CronParsing.Model;
using SteerMyWheel.CronParsing.Writers.Neo4j;
using SteerMyWheel.Discovery.ScriptToRepository;
using SteerMyWheel.Reader;
using SteerMyWheel.TaskQueue;
using SteerMyWheel.Workers;
using SteerMyWheel.Workers.Git;
using System;
using System.Configuration;
using System.IO;
using System.Security.Authentication.ExtendedProtection;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.Metrics;
using SteerMyWheel.Connectivity;

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
                    .AddTransient<NeoClient>()
                    .AddTransient<Neo4jWriter>()
                    .AddTransient<ReaderStateContext>()
                    .AddTransient<CronReader>()
                    .AddTransient<CronParser>()
                    .AddTransient<ScriptRepositoryService>());
            var host = builder.Build();
            WorkQueue<GitMigrationWorker> q = new WorkQueue<GitMigrationWorker>(loggerFactory,100);
            var globalConfig = host.Services.GetRequiredService<GlobalConfig>();
            Console.Write(globalConfig.neo4jUsername);
            //var test = new GitMigrationWorker(new ScriptExecution("test", "", "tools", "", "", true));
            //q.Enqueue(test);
            //q.DeqeueAllAsync(CancellationToken.None);
            //Console.ReadKey();
            var _reader = host.Services.GetRequiredService<CronReader>();
            _reader.GetContext().Initialize(new RemoteHost("PRDFRTAPP901", "PRDFRTAPP901", 22, "kcm-front", "Supervision!"));
            _reader.Read("C:/scripts.txt");
            var syncService = host.Services.GetRequiredService<ScriptRepositoryService>();
            syncService.syncRepos(new RemoteHost("PRDFRTAPP901","PRDFRTAPP901",22,"kcm-front","Supervision!"));

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
                .AddTransient<Neo4jWriter>()
                .AddTransient<TestWorker>();
                

        }   
    }
}
