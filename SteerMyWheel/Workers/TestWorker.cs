using Microsoft.Extensions.Logging;
using SteerMyWheel.TaskQueue;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Workers
{
    public class TestWorker : BaseWorker, IQueuable
    {
        public ILogger<BaseWorker> Logger { get; set; }
        public int n { get; }
        public TestWorker(int _n)
        {
            n = _n;
        }

        public override void setLogger(ILogger<BaseWorker> logger)
        {
            Logger = logger;
        }

        public override Task doWork()
        {
            Logger.LogInformation($"Working ! + {n}");
            return Task.CompletedTask;
        }
    }
}
