using Microsoft.Extensions.Logging;
using SteerMyWheel.Core.Connectivity.ClientProviders;
using SteerMyWheel.Core.Discovery.Crontab.Reader;
using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Core.Model.WorkersQueue;
using SteerMyWheel.Core.Workers.Discovery;
using SteerMyWheel.Domain.Discovery.CronParsing;
using SteerMyWheel.Domain.Discovery.CronParsing.ReaderState;
using SteerMyWheel.Domain.Model.WorkerQueue;
using SteerMyWheel.Domain.Service;
using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SteerMyWheel.Core.Services
{
    public class CronDiscoveryService : IService
    {
        private readonly ILogger<CronDiscoveryService> _logger;
        private ILoggerFactory _loggerFactory;
        private SSHClientProvider _client;
        public WorkersQueue<CronDiscoveryWorker> _queue;
        private CronReader _cronReader;
        public CronDiscoveryService(ILogger<CronDiscoveryService> logger,WorkersQueue<CronDiscoveryWorker> queue, CronReader cronReader,SSHClientProvider client)
        {
            _logger = logger;
            _queue = queue;
            _cronReader = cronReader;
            _client = client;
        }

        public Task Discover(RemoteHost host)
        {
            _logger.LogInformation($"[{DateTime.UtcNow} Creating new discovery worker for host {host.RemoteIP} ...");
            var worker = new CronDiscoveryWorker(host, _cronReader);
            worker.setLogger(_loggerFactory.CreateLogger<CronDiscoveryWorker>());
            worker.SetClientProvider(_client);
           _queue.Enqueue(worker).Wait();
            return Task.CompletedTask;
        }

        public void setLoggerFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }
    }
}
