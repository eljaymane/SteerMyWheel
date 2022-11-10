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
        public int migrateScript(Script script,Host source,Host target)
        {
            var _script = new BufferBlock<Script>();
            var _target = new BufferBlock<Host>();
            var _source = new BufferBlock<Host>();
            var joinScriptSourceTarget = new JoinBlock<Script, Host, Host>();
            _script.LinkTo(joinScriptSourceTarget.Target1);
            _source.LinkTo(joinScriptSourceTarget.Target2);
            _source.LinkTo(joinScriptSourceTarget.Target3);
            var linkToTarget = new ActionBlock<JoinBlock<Script, Host, Host>>(data =>
            {
                createHostsRelationShip(data);
            });
            var unlinkFromSource = new TransformBlock<JoinBlock<Script, Host, Host>, JoinBlock<Script, Host, Host>>(new Func<JoinBlock<Script, Host, Host>, JoinBlock<Script, Host, Host>>(deleteHostsRelationship));
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

        private JoinBlock<Script,Host,Host> deleteHostsRelationship(JoinBlock<Script,Host,Host> joinScriptSourceTarget)
        {
            try
            {
                 _client.Cypher.OptionalMatch("(s:Script)<-[HOSTS]-(h:Host)")
                     .Where((Script s) => s.name == joinScriptSourceTarget.Receive().Item1.name)
                     .AndWhere((Host h) => h.Name == joinScriptSourceTarget.Receive().Item3.Name)
                     .Delete("HOSTS")
                     .ExecuteWithoutResultsAsync().Wait();
                return joinScriptSourceTarget;
            }catch(Exception e)
            {
                return null;
            }
        }

        private JoinBlock<Script,Host,Host> createHostsRelationShip(JoinBlock<Script,Host,Host> joinScriptSourceTarget)
        {
            try
            {
                _client.Cypher.Match("(s:Script)", "(h:Host)")
                    .Where((Script s) => s.name == joinScriptSourceTarget.Receive().Item1.name)
                    .AndWhere((Host h) => h.Name == joinScriptSourceTarget.Receive().Item3.Name)
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
