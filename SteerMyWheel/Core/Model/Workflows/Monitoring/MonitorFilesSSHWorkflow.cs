using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Core.Model.Workflows.Abstractions;
using SteerMyWheel.Infrastracture.Connectivity.ClientProviders;
using System;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.Monitoring
{
    public class MonitorFilesSSHWorkflow : AbstractMonitorFilesWorkflow
    {
        private RemoteHost _RemoteHost;
        private SSHClient Client;
        public MonitorFilesSSHWorkflow(SSHClient client, RemoteHost remoteHost, string[] Paths, string name, string description, DateTime executionDate, BaseWorkflow next, BaseWorkflow previous) : base(Paths, name, description, executionDate, next, previous)
        {
            Client = client;
            _RemoteHost = remoteHost;

        }

        public override Task ExecuteAsync(BaseWorkflowContext context)
        {
            MonitorAsync(context);
            return Task.CompletedTask;
        }

        public Task MonitorAsync(BaseWorkflowContext context)
        {
            using (var client = Client)
            {
                client.ConnectSFTP(_RemoteHost);
                return base.MonitorAsync(client.FileExists, client.DirectoryExists, context);
            }

        }


    }
}
