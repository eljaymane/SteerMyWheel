using Neo4jClient;
using SteerMyWheel.Reader;
using SteerMyWheel.Model;
using SteerMyWheel.Writer;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SteerMyWheel.Writers.Neo4j
{
    public class Neo4jWriter : IWriter<IWritable>,IDisposable
    {
        private readonly ILogger<Neo4jWriter> _logger;
        private GraphClient _graphClient;
        private readonly ReaderStateContext context;
        public Neo4jWriter(string rootUri,string username, string password,string database,ReaderStateContext _context)
        {
            _logger = _context._loggerFactory.CreateLogger<Neo4jWriter>();
            _logger.LogInformation("[{time}] Neo4jWriter => Initializing Neo4j connection to {rootUri} with user : {username}",DateTime.UtcNow,rootUri,username);
            this.context = _context;
            _graphClient = new GraphClient(new Uri(rootUri), username, password);
            _graphClient.ConnectAsync().Wait();
            _logger.LogInformation("[{time}] Neo4jWriter => Successfully connected to Neo4j server !", DateTime.UtcNow);
            _graphClient.DefaultDatabase = database;
        }

    
        public async Task WriteAsync(IWritable value)
        {
            switch (value)
            {
                case Script script:
                    _logger.LogInformation("[{time}] Neo4jWriter => Creating script... : {script}", DateTime.UtcNow, script.ToString());
                    this._graphClient.Cypher.Match("(host:Host)")
                        .Where((Host host) => host.Name == context.currentHostName)
                        .Create("(host)-[:HOSTS]->(script:Script $script)")
                        .WithParam("script", script)
                        .ExecuteWithoutResultsAsync().Wait();
                    _logger.LogInformation("[{time}] Neo4jWriter => Successfully created script {scriptName}", DateTime.UtcNow,script.name);
                        break;
                   
                case Host host:
                    _logger.LogInformation("[{time}] Neo4jWriter => Creating host... : {host}", DateTime.UtcNow, host.ToString());
                    await this._graphClient.Cypher.Create("(host:Host $host)")
                        .WithParam("host", host)
                        .ExecuteWithoutResultsAsync();
                    _logger.LogInformation("[{time}] Neo4jWriter => Successfully created host {hostName}", DateTime.UtcNow, host.Name);
                    break;
                     
            }
        }
        public void Dispose()
        {
            //_graphClient.Transaction.CommitAsync().Wait();
            //this._graphClient.EndTransaction();
            this._graphClient.Dispose();
        }


    }
}
