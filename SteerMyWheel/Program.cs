using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Neo4jClient.Cypher;
using SteerMyWheel.CronParsing;
using SteerMyWheel.CronParsing.Model;
using SteerMyWheel.CronParsing.Writers.Neo4j;
using SteerMyWheel.Reader;
using SteerMyWheel.TaskQueue;
using SteerMyWheel.Workers;
using SteerMyWheel.Workers.Git;
using System;
using System.IO;
using System.Security.Authentication.ExtendedProtection;
using System.Threading;
using System.Threading.Tasks;

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

            WorkQueue<GitMigrationWorker> q = new WorkQueue<GitMigrationWorker>(loggerFactory,100);

            var test = new GitMigrationWorker(new Script("test", "", "tools", "", "", true));
            q.Enqueue(test);
            q.DeqeueAllAsync(CancellationToken.None);
            Console.ReadKey();

            //CronReader _reader = new CronReader("C:/scripts.txt", new Host("PRDFRTAPP901", "PRDFRTAPP901", 22, "KCH-FRONT", "Supervision!"),loggerFactory);
            //_reader.Read();


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
