using Neo4jClient;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SteerMyWheel.Core.Connectivity.Repositories;
using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Domain.Model.Writer;
using SteerMyWheel.Domain.Discovery.CronParsing.ReaderState;

namespace SteerMyWheel.Core.Discovery.Crontab.GraphWriter
{
    public class CronGraphWriter : IWriter<IWritable>, IDisposable
    {
        private readonly ILogger<CronGraphWriter> _logger;
        private readonly ScriptExecutionRepository _scriptExecutionRepository;
        private readonly RemoteHostRepository _remoteHostRepository;
        private ReaderStateContext context;
        public CronGraphWriter(ScriptExecutionRepository scriptExecutionRepository, RemoteHostRepository remoteHostRepository, ILogger<CronGraphWriter> logger)
        {
            _logger = logger;
            _scriptExecutionRepository = scriptExecutionRepository;
            _remoteHostRepository = remoteHostRepository;
        }

        public void setContext(ReaderStateContext context)
        {
            this.context = context;
        }
        public Task WriteHost(RemoteHost host)
        {
            _remoteHostRepository.Create(host);
            _logger.LogInformation("[{time}] Neo4jWriter => Successfully created RemoteHost {hostName}", DateTime.UtcNow, host.Name);
            return Task.CompletedTask;
        }

        public Task WriteScriptExecution(ScriptExecution entity, string remoteHostName)
        {
            _scriptExecutionRepository.CreateAndMatch(entity, remoteHostName);
            _logger.LogInformation("[{time}] Neo4jWriter => Successfully created ScriptExecution {script} and matched it to RemoteHost {host}", DateTime.UtcNow, entity.Name, remoteHostName);
            return Task.CompletedTask;
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }


    }
}
