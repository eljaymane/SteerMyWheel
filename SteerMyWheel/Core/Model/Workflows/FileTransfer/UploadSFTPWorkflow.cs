using SteerMyWheel.Domain.Model.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.FileTransfer
{
    public class UploadSFTPWorkflow : SSHWorkflow
    {
        public string LocalPath { get; set; }
        public string RemotePath { get; set; }
        public UploadSFTPWorkflow(string name, string description, DateTime executionDate, BaseWorkflow next, BaseWorkflow previous) : base(name, description, executionDate, next, previous)
        {
        }

        public override bool CanExecute()
        {
            return true;
        }

        public override Task Execute()
        {
            _sshClient.Upload(RemotePath, LocalPath).Wait();
             return Task.CompletedTask;
        }

        public override async Task ExecuteAsync()
        {
            await _sshClient.Upload(RemotePath, LocalPath);
        }
    }
}
