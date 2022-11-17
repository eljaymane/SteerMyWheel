using Microsoft.Extensions.Logging;
using SteerMyWheel.Core.Connectivity.ClientProviders;
using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Domain.Connectivity.GraphRepository;
using System;
using System.Linq;

namespace SteerMyWheel.Core.Connectivity.Repositories
{
    public class ScriptRepositoryRepository : BaseGraphRepository<ScriptRepository, string>
    {
        private readonly ILogger<ScriptRepositoryRepository> _logger;
        public ScriptRepositoryRepository(NeoClientProvider client, ILogger<ScriptRepositoryRepository> logger) : base(client)
        {
            _logger = logger;
        }

        public override ScriptRepository Create(ScriptRepository entity)
        {

            using (var client = _client.GetConnection())
            {
                try
                {
                    var result = client.Cypher.Create("(scriptRepository:ScriptRepository) $entity")
                    .WithParam("entity", entity)
                    .Return(s => s.As<ScriptRepository>()).ResultsAsync.Result.First();
                    return result;
                }
                catch (Exception e)
                {

                }

            }
            return default;
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
                    var entity = client.Cypher.Match("(scriptRepository:ScriptRepository)")
                         .Where((ScriptRepository s) => s.Name == X)
                         .Return(s => s.As<ScriptRepository>()).ResultsAsync.Result.First();
                    return entity;
                }
                catch (Exception e)
                {

                }
            }
            return default;
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
