using SteerMyWheel.Core.Connectivity.ClientProviders;
using SteerMyWheel.Core.Model.CronReading;
using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Domain.Model.WorkerQueue;
using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SteerMyWheel.Core.Workers.Discovery
{
    public class CronDiscoveryWorker : BaseWorker
    {
        private readonly RemoteHost _remoteHost;
        private SSHClientProvider _sshClient;
        private CronReader _cronReader;

        public CronDiscoveryWorker(RemoteHost remoteHost, CronReader cronReader)
        {
            _remoteHost = remoteHost;
            _cronReader = cronReader;
            _cronReader.GetContext().Initialize(remoteHost);
        }

        public override async Task doWork()
        {
             await DiscoverAsync(_remoteHost);
        }

        public async Task DiscoverAsync(RemoteHost host)
        {
            var getCron = new TransformBlock<RemoteHost, string>(new Func<RemoteHost, Task<string>>(GetCronAsync));
            var readCron = new ActionBlock<string>(async data =>
            {
                await ReadCronAsync(data);
            });
            getCron.LinkTo(readCron);
            getCron.Completion.ContinueWith( delegate { readCron.Complete(); });
            getCron.Post(host);
            getCron.Complete();
            getCron.Completion.Wait();
            readCron.Completion.Wait();
            
            

        }

        public async Task<string> GetCronAsync(RemoteHost host)
        {
            await _sshClient.ConnectSSH(host);
            _cronReader.GetContext().Initialize(host);
            return await _sshClient.GetCronFile();

        }

        public async Task ReadCronAsync(string cronText)
        {
            await _cronReader.ReadFromText(cronText);
        }

        public void SetClientProvider(SSHClientProvider client)
        {
            this._sshClient = client;
        }
    }
}
