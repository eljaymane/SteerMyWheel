using SteerMyWheel.Core.Model.CronReading;
using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Core.Model.WorkersQueue;
using SteerMyWheel.Infrastracture.Connectivity.ClientProviders;
using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SteerMyWheel.Core.Workers.Discovery
{
    /// <summary>
    /// The worker who's role is to discover the cron file of a remote host through SSH.
    /// </summary>
    public class CronDiscoveryWorker : BaseWorker
    {
        private readonly RemoteHost _remoteHost;
        private SSHClient _sshClient;
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
            getCron.Completion.ContinueWith(delegate { readCron.Complete(); });
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

        public void SetClientProvider(SSHClient client)
        {
            this._sshClient = client;
        }
    }
}
