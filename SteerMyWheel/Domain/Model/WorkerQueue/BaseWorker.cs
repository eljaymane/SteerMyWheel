using Microsoft.Extensions.Logging;
using Neo4jClient;
using SteerMyWheel.Configuration;
using SteerMyWheel.Domain.Connectivity.ClientProvider;
using System.Net.Http;
using System.Threading.Tasks;

namespace SteerMyWheel.Domain.Model.WorkerQueue
{
    public abstract class BaseWorker : IQueuable
    {
        public ILogger<IQueuable> Logger { get; set; }
        public GlobalConfig _globalConfig;
        public IClientProvider<GraphClient> _client;
        public IClientProvider<HttpClient> _BitClient;

        public abstract Task doWork();

        public virtual void setLogger(ILogger<BaseWorker> logger)
        {
            Logger = logger;
        }

        public virtual void setClient(IClientProvider<GraphClient> client)
        {
            _client = client;
        }

        public virtual void setBitBucketClient(IClientProvider<HttpClient> client)
        {
            _BitClient = client;
        }
    }
}
