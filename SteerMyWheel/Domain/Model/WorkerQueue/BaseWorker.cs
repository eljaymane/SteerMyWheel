using Microsoft.Extensions.Logging;
using Neo4jClient;
using Renci.SshNet;
using SteerMyWheel.Configuration;
using SteerMyWheel.Core.Connectivity.ClientProviders;
using SteerMyWheel.Core.Discovery.Crontab.Reader;
using SteerMyWheel.Domain.Connectivity.ClientProvider;
using SteerMyWheel.Infrastracture.Connectivity.ClientProviders;
using System.Net.Http;
using System.Threading.Tasks;

namespace SteerMyWheel.Domain.Model.WorkerQueue
{
    public abstract class BaseWorker : IQueuable
    {
        public ILogger<IQueuable> _logger { get; set; }
        public GlobalConfig _globalConfig;
        public IClientProvider<GraphClient> _client;
        public IClientProvider<HttpClient> _BitClient;
        public SSHClientProvider _sshClient;

        public abstract Task doWork();

        public virtual void setLogger(ILogger<BaseWorker> logger)
        {
            _logger = logger;
        }

        public virtual void setClient(IClientProvider<GraphClient> client)
        {
            _client = client;
        }

        public virtual void setBitBucketClient(IClientProvider<HttpClient> client)
        {
            _BitClient = client;
        }

        public virtual void setSSHClient(SSHClientProvider client)
        {
            _sshClient = client;
        }
    }
}
