﻿using Microsoft.Extensions.Logging;
using SteerMyWheel.Core.Model.CronReading;
using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Core.Model.WorkersQueue;
using SteerMyWheel.Core.Workers.Discovery;
using SteerMyWheel.Infrastracture.Connectivity.ClientProviders;
using System;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Services
{
    /// <summary>
    /// The service who's role is to discover the cron file of a remote host, parse it and represents it's elements on a graph database.
    /// This class uses a ReaderStateContext encapsulated in a CronReader instance.
    /// This class generates CronDiscoveryWorkers and queue them in a WorkersQueue.
    /// </summary>
    public class CronDiscoveryService
    {
        private readonly ILogger<CronDiscoveryService> _logger;
        private ILoggerFactory _loggerFactory;
        private SSHClient _client;
        public WorkersQueue<CronDiscoveryWorker> _queue;
        private CronReader _cronReader;
        public CronDiscoveryService(ILogger<CronDiscoveryService> logger, WorkersQueue<CronDiscoveryWorker> queue, CronReader cronReader, SSHClient client)
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
