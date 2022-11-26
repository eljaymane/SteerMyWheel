using Microsoft.Extensions.Logging;
using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Infrastracture.Connectivity.ClientProviders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SteerMyWheel.Infrastracture.Connectivity.Repositories
{
    public class RemoteHostRepository : BaseGraphRepository<RemoteHost, string>
    {
        private readonly ILogger<RemoteHostRepository> _logger;

        public RemoteHostRepository() : base()
        {

        }
        public RemoteHostRepository(NeoClientProvider client, ILogger<RemoteHostRepository> logger) : base(client)
        {
            _logger = logger;
            try
            {
                _client.GetConnection().Cypher.CreateUniqueConstraint("h:RemoteHost", "h.Name").ExecuteWithoutResultsAsync().Wait();
                _logger.LogInformation("[{time}] Created unique constraint on RemoteHost.Name ...", DateTime.UtcNow);
            }
            catch (Exception e)
            {
                _logger.LogInformation("[{time}] Unique constraint on ScriptExecution.ExecCommand already exists !", DateTime.UtcNow);
            }
        }

        public override RemoteHost Create(RemoteHost entity)
        {
            _logger.LogInformation("[{time}] RemoteHostRepository => Creating host : {host}", DateTime.UtcNow, entity.ToString());
            using (var client = _client.GetConnection())
            {
                try
                {
                    client.Cypher.Merge("(remoteHost:RemoteHost { Name : $name })")
                         .OnCreate()
                         .Set("remoteHost = $entity")
                         .WithParams(new
                         {
                             name = entity.Name,
                             entity
                         })
                          .ExecuteWithoutResultsAsync().Wait();
                    return entity;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }
        public override BaseEntity<string> CreateAndMatch(BaseEntity<string> newScript, string id)
        {
            throw new NotImplementedException();
        }

        public override RemoteHost Delete(RemoteHost entity)
        {
            using (var client = _client.GetConnection())
            {
                try
                {
                    client.Cypher.Match("(remoteHost:RemoteHost)")
                         .Where((RemoteHost h) => h.RemoteIP == entity.RemoteIP)
                         .Delete("remoteHost");
                    return entity;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        public override RemoteHost Get(string X)
        {
            using (var client = _client.GetConnection())
            {
                try
                {
                    var entity = client.Cypher.Match("(r:RemoteHost)")
                         .Where((RemoteHost r) => r.RemoteIP == X)
                         .Return(r => r.As<RemoteHost>()).ResultsAsync.Result.First();
                    return entity;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        public override RemoteHost Get(object o)
        {
            var repository = o as ScriptRepository;
            using (var client = _client.GetConnection())
            {
                try
                {
                    var entity = client.Cypher.Match("(r:RemoteHost)-[:HOSTS]-(se:ScriptExecution)-[:IS_ON]->(s:ScriptRepository)")
                         .Where((ScriptRepository s) => s.Name == repository.Name)
                         .Return(r => r.As<RemoteHost>()).ResultsAsync.Result.First();
                    return entity;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        public override IEnumerable<RemoteHost> GetAll()
        {
            using (var client = _client.GetConnection())
            {
                try
                {
                    var entities = client.Cypher.Match("(remoteHost:RemoteHost)")
                         .Return(s => s.As<RemoteHost>()).ResultsAsync.Result;
                    return entities;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }
        public override IEnumerable<RemoteHost> GetAll(object entity)
        {
            throw new NotImplementedException();
        }

        public override Tuple<BaseEntity<string>, object> Link(BaseEntity<string> active, object passive)
        {
            throw new NotImplementedException();
        }

        public override RemoteHost Update(RemoteHost entity)
        {
            using (var client = _client.GetConnection())
            {
                try
                {
                    var result = client.Cypher.Match("(remoteHost:RemoteHost)")
                         .Where((RemoteHost s) => s.RemoteIP == entity.RemoteIP)
                         .Return(s => s.As<RemoteHost>()).ResultsAsync.Result.First();
                    return result;
                }
                catch (Exception e)
                {
                    return null;
                }
            }

        }
    }
}
