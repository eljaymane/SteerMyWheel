using Microsoft.Extensions.Logging;
using Neo4jClient;
using SteerMyWheel.Connectivity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.TaskQueue
{
    public abstract class BaseWorker : IQueuable
    {
        public ILogger<IQueuable> Logger { get; set; }
        public GlobalConfig _globalConfig;
        public IClientProvider<GraphClient> _client;

        public abstract Task doWork();

        public virtual void setLogger(ILogger<BaseWorker> logger)
        {
            Logger = logger;
        }

        public virtual void setClient(IClientProvider<GraphClient> client)
        {
            _client = client;
        }
    }
}
