using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using SteerMyWheel.Core.Connectivity.ClientProviders;
using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Domain.Connectivity.GraphRepository;
using SteerMyWheel.Infrastracture.Connectivity.ClientProviders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SteerMyWheel.Core.Connectivity.Repositories
{
    public class ScriptRepositoryRepository : BaseGraphRepository<ScriptRepository, string>
    {
        private readonly ILogger<ScriptRepositoryRepository> _logger;
        public ScriptRepositoryRepository(NeoClientProvider client, ILogger<ScriptRepositoryRepository> logger) : base(client)
        {
            _logger = logger;
            //Setting unique constraints for ScriptRepository on parameter name to avoid duplicates
            try
            {
                _client.GetConnection().Cypher.CreateUniqueConstraint("s:ScriptRepository", "s.Name").ExecuteWithoutResultsAsync().Wait();
                _logger.LogInformation("[{time}] Created unique constraint on ScriptRepository.name  ...", DateTime.UtcNow);
            }
            catch (Exception e)
            {
                _logger.LogInformation("[{time}] Unique constraint on ScriptRepository.Name already exists !", DateTime.UtcNow);
            }
        }

        public override ScriptRepository Create(ScriptRepository entity)
        {

            using (var client = _client.GetConnection())
            {
                try
                {
                    client.Cypher.Create("(s:ScriptRepository) $s")
                   .WithParam("s", entity)
                   .ExecuteWithoutResultsAsync().Wait();
                    _logger.LogInformation("[{time}] Created new ScriptRepository {name}  ...", DateTime.UtcNow, entity.Name);
                    return entity;
                }
                catch (Exception e)
                {
                    _logger.LogError("[{time}] ScriptRepository {name} already exists !", DateTime.UtcNow, entity.Name);
                    return null;
                }

            }
        }

        public Tuple<ScriptRepository,ScriptExecution> Link(ScriptRepository scriptRepository, ScriptExecution scriptExecution)
        {
            using (var client = _client.GetConnection())
            {
                try
                {
                    client.Cypher.Match("(s:ScriptExecution)")
                         .Where((ScriptExecution s) => s.ExecCommand == scriptExecution.ExecCommand)
                         .Create("(s)-[:IS_ON]->(r:ScriptRepository $r)")
                         .WithParam("r",scriptRepository)
                         .ExecuteWithoutResultsAsync().Wait();
                    _logger.LogInformation("[{time}] Successfully linked ScriptExecution {ScriptName} to ScriptRepository {name}  ...", DateTime.UtcNow, scriptExecution.Name, scriptRepository.Name);
                    return new Tuple<ScriptRepository,ScriptExecution>(scriptRepository,scriptExecution);
                }
                catch (Exception e)
                {
                    _logger.LogError("[{time}] Could not link ScriptExecution {ScriptName} to ScriptRepository {name}  ...", DateTime.UtcNow, scriptExecution.Name, scriptRepository.Name);
                    return null;
                }

            }

        }

        public override ScriptRepository Delete(ScriptRepository entity)
        {
            using (var client = _client.GetConnection())
            {
                try
                {
                    client.Cypher.Delete("(scriptRepository:ScriptRepository)")
                        .Where((ScriptRepository s) => s.Equals(entity))
                        .ExecuteWithoutResultsAsync().Wait();
                }
                catch (Exception e)
                {
                    return default;
                }
                return entity;
            }
        }

        public override ScriptRepository Get(string X)
        {
            using (var client = _client.GetConnection())
            {
                try
                {
                    var entity = client.Cypher.Match("(s:ScriptRepository)")
                         .Where((ScriptRepository s) => s.Name == X)
                         .Return(s => s.As<ScriptRepository>()).ResultsAsync.Result.First();
                    return entity;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        public override IEnumerable<ScriptRepository> GetAll()
        {

            using (var client = _client.GetConnection())
            {
                try
                {
                    var entities = client.Cypher.Match("(scriptRepository:ScriptRepository)")
                         .Return(s => s.As<ScriptRepository>()).ResultsAsync.Result;
                    return entities;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        public IEnumerable<ScriptRepository> GetAll(RemoteHost host)
        { 
            using(var client = _client.GetConnection())
            {
                try
                {
                    var entities = client.Cypher
                                       .Match("(h:RemoteHost)-[:HOSTS]-(se:ScriptExecution)-[:IS_ON]->(s:ScriptRepository)")
                                       .Where((RemoteHost h) => h.Name == host.Name)
                                       .ReturnDistinct(s => s.As<ScriptRepository>())
                                       .ResultsAsync.Result;
                    _logger.LogInformation("[{time}] Retrieved {count} ScriptRepositories for RemoteHost {host}...", DateTime.UtcNow, entities.Count(),host.Name);
                    return entities;
                }catch(Exception e)
                {
                    return null;
                }
                
            }
           
        }

        public override ScriptRepository Update(ScriptRepository entity)
        {
            using (var client = _client.GetConnection())
            {
                try
                {
                    client.Cypher.Match("(scriptRepository:ScriptRepository)")
                          .Where((ScriptRepository s) => s.Name == entity.Name)
                          .Set("(scriptRepository = $entity")
                          .WithParam("entity", entity)
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
