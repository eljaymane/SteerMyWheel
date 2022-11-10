using Neo4jClient;
using SteerMyWheel.CronParsing.Model;
using SteerMyWheel.ScriptsHandling.Clients;
using SteerMyWheel.TaskQueue;
using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SteerMyWheel.Workers
{
    public class MigrationWorker : IQueuable
    {
        public static SSHClient _sshClient;
        public static GraphDAO _graphClient;

        private Script _script;
        private Host _source;
        private Host _target;
        public MigrationWorker(Script script, Host source, Host target)
        {
            _sshClient = new SSHClient();
            _graphClient = new GraphDAO(new GraphClient("http://localhost:7474/", "neo4j", "Supervision!"), "neo4j");
            _script = script;
            _source = source;
            _target = target;
        }

        public async Task doWork()
        {
            var joinData = new JoinBlock<Script, Host, Host>();
            joinData.Target1.Post(_script);
            joinData.Target2.Post(_source);
            joinData.Target3.Post(_target);
            try
            {
                var doMigration = new TransformBlock<JoinBlock<Script, Host, Host>, JoinBlock<Script, Host, Host>>(new Func<JoinBlock<Script, Host, Host>, JoinBlock<Script, Host, Host>>(_sshClient.doSSHMigration));
                var reflectChangeToDB = new ActionBlock<JoinBlock<Script, Host, Host>>(data =>
                {
                    _graphClient.migrateScript(data.Receive().Item1, data.Receive().Item2, data.Receive().Item3);
                });
                doMigration.LinkTo(reflectChangeToDB);
                doMigration.Completion.ContinueWith(delegate { reflectChangeToDB.Complete(); });
                doMigration.Post(joinData);
                doMigration.Complete();
                reflectChangeToDB.Completion.Wait();
            }
            catch (Exception e)
            {
            }



        }
    }
}
