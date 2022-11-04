using Neo4jClient;
using SteerMyWheel.Reader;
using SteerMyWheel.Model;
using SteerMyWheel.Writer;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Writers.Neo4j
{
    public class Neo4jWriter : IWriter<IWritable>,IDisposable
    {
        private GraphClient _graphClient;
        private readonly ReaderStateContext context;
        public Neo4jWriter(string rootUri,string username, string password,string database,ReaderStateContext _context)
        {
            this.context = _context;
            _graphClient = new GraphClient(new Uri(rootUri), username, password);
            _graphClient.ConnectAsync().Wait();
            _graphClient.DefaultDatabase = database;
        }

    
        public async Task WriteAsync(IWritable value)
        {
            switch (value)
            {
                case Script script:
                    await this._graphClient.Cypher.Match("(host:Host)")
                        .Where((Host host) => host.Name == context.currentHostName)
                        .Create("(host)-[:HOSTS]->(script:Script $script)")
                        .WithParam("script", script)
                        .ExecuteWithoutResultsAsync();
                        break;
                   
                case Host host:
                    await this._graphClient.Cypher.Create("(host:Host $host)")
                        .WithParam("host", host)
                        .ExecuteWithoutResultsAsync();
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
