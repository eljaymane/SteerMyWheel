using Microsoft.Extensions.Logging;
using SteerMyWheel.Configuration;
using SteerMyWheel.Infrastracture.Connectivity.ClientProviders;

namespace SteerMyWheel.Infrastracture.Connectivity
{
    public class SSHClientFactory
    {
        private ILoggerFactory LoggerFactory;
        private GlobalConfig Config;
        private static SSHClientFactory instance;

        public SSHClientFactory(ILoggerFactory loggerFactory, GlobalConfig config)
        {
            LoggerFactory = loggerFactory;
            Config = config;
            instance = this;
        }

        public SSHClientFactory GetInstance() { return instance; }

        public SSHClient CreateSSHClient()
        {
            return new SSHClient(Config, LoggerFactory.CreateLogger<SSHClient>());
        }
    }
}
