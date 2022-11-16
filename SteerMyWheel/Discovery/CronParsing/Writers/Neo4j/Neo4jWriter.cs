using Neo4jClient;
using SteerMyWheel.Model;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SteerMyWheel.Reader;
using SteerMyWheel.Writer;
using SteerMyWheel.Connectivity.ClientProviders;

namespace SteerMyWheel.CronParsing.Writers.Neo4j
{
    public class Neo4jWriter : IWriter<IWritable>,IDisposable
    {
        private readonly ILogger<Neo4jWriter> _logger;
        private GraphClient _graphClient;
        private ReaderStateContext context;
        public Neo4jWriter(NeoClientProvider _client,ILogger<Neo4jWriter> logger)
        {
            _logger = logger;
            _graphClient = _client.GetConnection();

        }

        public void setContext(ReaderStateContext context)
        {
            this.context = context;
        }
        public async Task WriteAsync(IWritable value)
        {
            switch (value)
            {
                case ScriptExecution script:
                    _logger.LogInformation("[{time}] Neo4jWriter => Creating script... : {script}", DateTime.UtcNow, script.ToString());
                    this._graphClient.Cypher.Match("(host:RemoteHost)")
                        .Where((RemoteHost host) => host.Name == context.currentHostName)
                        .Create("(host)-[:HOSTS]->(script:ScriptExecution $script)")
                        .WithParam("script", script)
                        .ExecuteWithoutResultsAsync().Wait();
                    _logger.LogInformation("[{time}] Neo4jWriter => Successfully created script {scriptName}", DateTime.UtcNow,script.Name);
                        break;
                   
                case RemoteHost host:
                    _logger.LogInformation("[{time}] Neo4jWriter => Creating host... : {host}", DateTime.UtcNow, host.ToString());
                    this._graphClient.Cypher.Create("(host:RemoteHost $host)")
                        .WithParam("host", host)
                        .ExecuteWithoutResultsAsync().Wait();
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
