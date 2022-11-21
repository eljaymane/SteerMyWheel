using Microsoft.Extensions.Logging;
using Neo4jClient;
using SteerMyWheel.Configuration;
using SteerMyWheel.Domain.Connectivity.ClientProvider;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Infrastracture.Connectivity.ClientProviders
{
    public class NeoClientProvider : BaseClientProvider<GraphClient>
    {
        private readonly ILogger<NeoClientProvider> _logger;
        private readonly GlobalConfig _config;
        private GraphClient _client;

        public NeoClientProvider(GlobalConfig config, ILogger<NeoClientProvider> logger)
        {
            _config = config;
            _logger = logger;
            _logger.LogInformation("[{time}] Neo4jWriter => Initializing Neo4j connection to {rootUri} with user : {username}", DateTime.UtcNow, _config.neo4jRootURI, _config.neo4jUsername);
            _client = new GraphClient(_config.neo4jRootURI, _config.neo4jUsername, _config.neo4jPassword);
        }

        public override GraphClient GetConnection()
        {
            if (!_client.IsConnected) Connect();
            return _client;

        }

        public override Task Connect()
        {

            _client.DefaultDatabase = _config.neo4jDefaultDB;
            _client.ConnectAsync().Wait();
            _logger.LogInformation("[{time}] Neo4jWriter => Successfully connected to Neo4j server !", DateTime.UtcNow);
            return Task.CompletedTask;

        }

        public override void Dispose()
        {
            _client.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
