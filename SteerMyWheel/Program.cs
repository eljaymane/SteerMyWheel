using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SteerMyWheel.Model;
using SteerMyWheel.Reader;
using SteerMyWheel.Writers.Neo4j;
using System;
using System.IO;
using System.Security.Authentication.ExtendedProtection;

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
            CronReader _reader = new CronReader("C:/scripts.txt", new Host("PRDFRTAPP901", "PRDFRTAPP901", 22, "KCH-FRONT", "Supervision!"),loggerFactory);
            _reader.Read();

            
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddConsole())
                .AddTransient<ReaderStateContext>()
                .AddTransient<CronParser>()
                .AddTransient<Neo4jWriter>();
                

        }   
    }
}
