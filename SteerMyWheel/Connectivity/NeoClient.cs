using Microsoft.Extensions.Logging;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteerMyWheel.Connectivity
{
    public class NeoClient : IClientProvider<GraphClient>
    {
        private readonly ILogger<NeoClient> _logger;
        private readonly GlobalConfig _config;
        private GraphClient _client;

        public NeoClient(GlobalConfig config, ILogger<NeoClient> logger)
        {
            _config = config;
            _logger = logger;
            _logger.LogInformation("[{time}] Neo4jWriter => Initializing Neo4j connection to {rootUri} with user : {username}", DateTime.UtcNow, _config.neo4jRootURI, _config.neo4jPassword);
            _client = new GraphClient(_config.neo4jRootURI, _config.neo4jUsername, _config.neo4jPassword);
        }

        public GraphClient GetConnection()
        {
            if (!_client.IsConnected) Connect();
            return _client;

        }

        public void Connect()
        {
           
            _client.DefaultDatabase = _config.neo4jDefaultDB;
            _client.ConnectAsync().Wait();
            _logger.LogInformation("[{time}] Neo4jWriter => Successfully connected to Neo4j server !", DateTime.UtcNow);

        }

    }
}
