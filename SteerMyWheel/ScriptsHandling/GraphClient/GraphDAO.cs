using Neo4jClient;
using Neo4jClient.Cypher;
using SteerMyWheel.CronParsing.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SteerMyWheel.ScriptsHandling.Clients
{
    public class GraphDAO
    {
        private GraphClient _client;

        public GraphDAO(GraphClient client,string defaultDB)
        {
            _client = client;
            _client.DefaultDatabase = defaultDB;
        }
        public int migrateScript(ScriptExecution script,RemoteHost source,RemoteHost target)
        {
            var _script = new BufferBlock<ScriptExecution>();
            var _target = new BufferBlock<RemoteHost>();
            var _source = new BufferBlock<RemoteHost>();
            var joinScriptSourceTarget = new JoinBlock<ScriptExecution, RemoteHost, RemoteHost>();
            _script.LinkTo(joinScriptSourceTarget.Target1);
            _source.LinkTo(joinScriptSourceTarget.Target2);
            _source.LinkTo(joinScriptSourceTarget.Target3);
            var linkToTarget = new ActionBlock<JoinBlock<ScriptExecution, RemoteHost, RemoteHost>>(data =>
            {
                createHostsRelationShip(data);
            });
            var unlinkFromSource = new TransformBlock<JoinBlock<ScriptExecution, RemoteHost, RemoteHost>, JoinBlock<ScriptExecution, RemoteHost, RemoteHost>>(new Func<JoinBlock<ScriptExecution, RemoteHost, RemoteHost>, JoinBlock<ScriptExecution, RemoteHost, RemoteHost>>(deleteHostsRelationship));
            unlinkFromSource.LinkTo(linkToTarget);
            unlinkFromSource.Completion.ContinueWith(delegate { linkToTarget.Complete(); });
            try
            {
                _script.Post(script);
                _source.Post(source);
                _target.Post(target);
                unlinkFromSource.Post(joinScriptSourceTarget);
                unlinkFromSource.Complete();
                linkToTarget.Completion.Wait();
                return 1;

            }catch(Exception e)
            {
                return -1;
            }
        }

        private JoinBlock<ScriptExecution,RemoteHost,RemoteHost> deleteHostsRelationship(JoinBlock<ScriptExecution,RemoteHost,RemoteHost> joinScriptSourceTarget)
        {
            try
            {
                 _client.Cypher.OptionalMatch("(s:Script)<-[HOSTS]-(h:Host)")
                     .Where((ScriptExecution s) => s.name == joinScriptSourceTarget.Receive().Item1.name)
                     .AndWhere((RemoteHost h) => h.name == joinScriptSourceTarget.Receive().Item3.name)
                     .Delete("HOSTS")
                     .ExecuteWithoutResultsAsync().Wait();
                return joinScriptSourceTarget;
            }catch(Exception e)
            {
                return null;
            }
        }

        private JoinBlock<ScriptExecution,RemoteHost,RemoteHost> createHostsRelationShip(JoinBlock<ScriptExecution,RemoteHost,RemoteHost> joinScriptSourceTarget)
        {
            try
            {
                _client.Cypher.Match("(s:Script)", "(h:Host)")
                    .Where((ScriptExecution s) => s.name == joinScriptSourceTarget.Receive().Item1.name)
                    .AndWhere((RemoteHost h) => h.name == joinScriptSourceTarget.Receive().Item3.name)
                    .CreateUnique("h-[:HOSTS]->s")
                    .ExecuteWithoutResultsAsync().Wait();
                return joinScriptSourceTarget;
            } catch(Exception e)
            {
                return null;
            }
        }
    }
}
