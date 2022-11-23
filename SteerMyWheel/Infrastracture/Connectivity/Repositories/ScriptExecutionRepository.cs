using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SteerMyWheel.Core.Connectivity.ClientProviders;
using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Domain.Connectivity.GraphRepository;
using SteerMyWheel.Infrastracture.Connectivity.ClientProviders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SteerMyWheel.Core.Connectivity.Repositories
{
    public class ScriptExecutionRepository : BaseGraphRepository<ScriptExecution, string>
    {
        private readonly ILogger<ScriptExecutionRepository> _logger;
        public ScriptExecutionRepository(NeoClientProvider client,ILogger<ScriptExecutionRepository> logger) : base(client)
        {
            _logger = logger;
            //Creating unique constraint for ScriptExection on parameter ExecCommand to avoid duplicates
            try
            {
                _client.GetConnection().Cypher.CreateUniqueConstraint("s:ScriptExecution", "s.ExecCommand").ExecuteWithoutResultsAsync().Wait();
                _logger.LogInformation("[{time}] Created unique constraint on ScriptExecution.ExecCommand  ...", DateTime.UtcNow);
            }
            catch (Exception e)
            {
                _logger.LogInformation("[{time}] Unique constraint on ScriptExecution.ExecCommand already exists !", DateTime.UtcNow);
            }
        }

        public override ScriptExecution Create(ScriptExecution entity)
        {
            using (var client = _client.GetConnection())
            {
                try
                {
                    client.Cypher.Create("(scriptExecution:ScriptExecution) $entity")
                    .WithParam("entity", entity)
                    .ExecuteWithoutResultsAsync().Wait();
                    _logger.LogInformation($"[{DateTime.UtcNow}] Created ScriptExecution {entity.Name} !");
                }
                catch (Exception e)
                {
                    return null;
                }

            }
            return entity;
        }

        public override ScriptExecution Delete(ScriptExecution entity)
        {
            using (var client = _client.GetConnection())
            {
                try
                {
                    client.Cypher.Delete("(scriptExecution:ScriptExecution)")
                        .Where((ScriptRepository s) => s.Equals(entity))
                        .ExecuteWithoutResultsAsync().Wait();
                }
                catch (Exception e)
                {
                    return null;
                }
                return entity;
            }
        }

        public override ScriptExecution Get(string X)
        {
            using (var client = _client.GetConnection())
            {
                try
                {
                    var entity = client.Cypher.Match("(scriptExecution:ScriptExecution)")
                         .Where((ScriptExecution s) => s.ExecCommand == X)
                         .Return(s => s.As<ScriptExecution>()).ResultsAsync.Result.First();
                    return entity;
                }
                catch (Exception e)
                {

                }
            }
            return default;
        }

        public override IEnumerable<ScriptExecution> GetAll()
        {
            using (var client = _client.GetConnection())
            {
                try
                {
                    var entities = client.Cypher.Match("(scriptExecution:ScriptExecution)")
                         .Return(scriptExecution => scriptExecution.As<ScriptExecution>()).ResultsAsync.Result;
                    return entities;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        public IEnumerable<ScriptExecution> GetAll(RemoteHost host)
        {
            using (var client = _client.GetConnection())
            {
                try
                {
                    _logger.LogInformation("[{time}] Requesting all executions for host {host} ...", DateTime.UtcNow, host.Name);
                    var entities = client.Cypher.Match("(h:RemoteHost)-[:HOSTS]->(scriptExecution:ScriptExecution)")
                        .Where((RemoteHost h) => h.Name == host.Name)
                        .Return(scriptExecution => scriptExecution.As<ScriptExecution>()).ResultsAsync.Result;
                    return entities;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        public IEnumerable<ScriptExecution> GetAll(ScriptRepository repository)
        {
            using (var client = _client.GetConnection())
            {
                try
                {
                    _logger.LogInformation("[{time}] Requesting all executions for repository {name} ...", DateTime.UtcNow, repository.Name);
                    var entities = client.Cypher.Match("(s:ScriptExecution)-[:IS_ON]->(r:ScriptRepository)")
                        .Where((ScriptRepository s) => s.Name == s.Name)
                        .Return(s => s.As<ScriptExecution>()).ResultsAsync.Result;
                    return entities;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        public override ScriptExecution Update(ScriptExecution entity)
        {
            using (var client = _client.GetConnection())
            {
                try
                {
                    client.Cypher.Match("(scriptExecution:ScriptExecution)")
                          .Where((ScriptExecution s) => s.ExecCommand == entity.ExecCommand)
                          .Set("scriptExecution = $entity")
                          .WithParam("entity", entity)
                          .ExecuteWithoutResultsAsync().Wait();
                    return entity;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
            return default;
        }

        public ScriptExecution CreateAndMatch(ScriptExecution entity, string remoteHostName)
        {
            using (var client = _client.GetConnection())
            {
                try
                {
                    client.Cypher.Match("(host:RemoteHost)")
                        .Where((RemoteHost host) => host.Name == remoteHostName)
                        .Create("(host)-[:HOSTS]->(script:ScriptExecution $script)")
                        .WithParam("script", entity)
                        .ExecuteWithoutResultsAsync().Wait();
                }
                catch (Exception e)
                {
                    return null;
                }
            }
            return entity;
        }
    }
}
