using SteerMyWheel.Connectivity.ClientProviders;
using SteerMyWheel.Model;
using System;
using System.Linq;

namespace SteerMyWheel.Connectivity.Repositories
{
    public class ScriptExecutionRepository : BaseGraphRepository<ScriptExecution, string>
    {
        public ScriptExecutionRepository(NeoClientProvider client) : base(client)
        {
        }

        public override ScriptExecution Create(ScriptExecution entity)
        {
            using (var client = base._client.GetConnection())
            {
                try
                {
                    client.Cypher.Create("(scriptExecution:ScriptExecution) $entity")
                    .WithParam("entity", entity)
                    .ExecuteWithoutResultsAsync().Wait();
                }catch(Exception e)
                {
                    return null;
                }
                
            }
            return entity;
        }

        public override ScriptExecution Delete(ScriptExecution entity)
        {
            using(var client = base._client.GetConnection())
            {
                try
                {
                    client.Cypher.Delete("(scriptExecution:ScriptExecution)")
                        .Where((ScriptRepository s) => s.Equals(entity))
                        .ExecuteWithoutResultsAsync().Wait();
                }catch(Exception e)
                {
                    return null;
                }
                return entity;
            }
        }

        public override ScriptExecution Get(string X)
        {
            using (var client = base._client.GetConnection())
            {
                try
                {
                    var entity = client.Cypher.Match("(scriptExecution:ScriptExecution)")
                         .Where((ScriptExecution s) => s.ExecCommand == X)
                         .Return<ScriptExecution>(s => s.As<ScriptExecution>()).ResultsAsync.Result.First();
                    return entity;
                }
                catch (Exception e)
                {

                }
            }
            return default(ScriptExecution);
        }

        public override ScriptExecution Update(ScriptExecution entity)
        {
            using (var client = base._client.GetConnection())
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
            return default(ScriptExecution);
        }
    }
}
