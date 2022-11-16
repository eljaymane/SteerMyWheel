using Neo4jClient;
using SteerMyWheel.Model;
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

        private ScriptExecution _script;
        private RemoteHost _source;
        private RemoteHost _target;
        public MigrationWorker(ScriptExecution script, RemoteHost source, RemoteHost target)
        {
            _sshClient = new SSHClient();
            _graphClient = new GraphDAO(new GraphClient("http://localhost:7474/", "neo4j", "Supervision!"), "neo4j");
            _script = script;
            _source = source;
            _target = target;
        }

        public async Task doWork()
        {
            var joinData = new JoinBlock<ScriptExecution, RemoteHost, RemoteHost>();
            joinData.Target1.Post(_script);
            joinData.Target2.Post(_source);
            joinData.Target3.Post(_target);
            try
            {
                var doMigration = new TransformBlock<JoinBlock<ScriptExecution, RemoteHost, RemoteHost>, JoinBlock<ScriptExecution, RemoteHost, RemoteHost>>(new Func<JoinBlock<ScriptExecution, RemoteHost, RemoteHost>, JoinBlock<ScriptExecution, RemoteHost, RemoteHost>>(_sshClient.doSSHMigration));
                var reflectChangeToDB = new ActionBlock<JoinBlock<ScriptExecution, RemoteHost, RemoteHost>>(data =>
                {
                    _graphClient.migrateScript(data.Receive().Item1, data.Receive().Item2, data.Receive().Item3);
                });
                doMigration.LinkTo(reflectChangeToDB);
                await doMigration.Completion.ContinueWith(delegate { reflectChangeToDB.Complete(); });
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
