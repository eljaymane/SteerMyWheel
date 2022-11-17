using Neo4jClient;
using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Core.Synchronization.Migration.SSH;
using SteerMyWheel.Domain.Model.WorkerQueue;
using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SteerMyWheel.Workers
{
    public class MigrationWorker : BaseWorker
    {
        public static SSHMigration _sshClient;
        public static GraphPhysicalMigration _graphClient;
        private ScriptExecution _script;
        private RemoteHost _source;
        private RemoteHost _target;
        public MigrationWorker(ScriptExecution script, RemoteHost source, RemoteHost target)
        {
            _sshClient = new SSHMigration();
            _script = script;
            _source = source;
            _target = target;
        }

        public override async Task doWork()
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
