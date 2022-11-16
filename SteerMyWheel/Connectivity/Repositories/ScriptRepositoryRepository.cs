using SteerMyWheel.Connectivity.ClientProviders;
using SteerMyWheel.Model;
using System;
using System.Linq;

namespace SteerMyWheel.Connectivity.Repositories
{
    public class ScriptRepositoryRepository : BaseGraphRepository<ScriptRepository, string>
    {
        public ScriptRepositoryRepository(NeoClientProvider client) : base(client)
        {
        }

        public override ScriptRepository Create(ScriptRepository entity)
        {
            
            using (var client = base._client.GetConnection())
            {
                try
                {
                    var result = client.Cypher.Create("(scriptRepository:ScriptRepository) $entity")
                    .WithParam("entity", entity)
                    .Return(s => s.As <ScriptRepository>()).ResultsAsync.Result.First();
                    return result;
                }
                catch (Exception e)
                {
                    
                }

            }
            return default(ScriptRepository);
        }

        public override ScriptRepository Delete(ScriptRepository entity)
        {
            using (var client = base._client.GetConnection())
            {
                try
                {
                    client.Cypher.Delete("(scriptRepository:ScriptRepository)")
                        .Where((ScriptRepository s) => s.Equals(entity))
                        .ExecuteWithoutResultsAsync().Wait();
                }
                catch (Exception e)
                {
                    return default(ScriptRepository);
                }
                return entity;
            }
        }

        public override ScriptRepository Get(String X)
        {
            using (var client = base._client.GetConnection())
            {
                try
                {
                    var entity = client.Cypher.Match("(scriptRepository:ScriptRepository)")
                         .Where((ScriptRepository s) => s.Name == X)
                         .Return<ScriptRepository>(s => s.As <ScriptRepository>()).ResultsAsync.Result.First();
                    return entity;
                }
                catch (Exception e)
                {
                   
                }
            }
            return default(ScriptRepository);
        }

        public override ScriptRepository Update(ScriptRepository entity)
        {
            using (var client = base._client.GetConnection())
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
