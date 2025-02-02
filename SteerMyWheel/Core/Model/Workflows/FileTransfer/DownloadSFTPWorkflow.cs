﻿using SteerMyWheel.Core.Model.Workflows.Abstractions;
using System;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.FileTransfer
{
    public class DownloadSFTPWorkflow : BaseSSHWorkflow
    {
        public string LocalPath { get; set; }
        public string RemotePath { get; set; }

        public DownloadSFTPWorkflow(string name, string description, DateTime executionDate, BaseWorkflow next, BaseWorkflow previous) : base(name, description, executionDate, next, previous)
        {

        }

        public override bool CanExecute()
        {
            return true;
        }

        public override Task Execute(BaseWorkflowContext context)
        {
            _sshClient.DownloadDirectory(RemotePath, LocalPath).Wait();
            return Task.CompletedTask;

        }

        public async override Task ExecuteAsync(BaseWorkflowContext context)
        {
            await _sshClient.DownloadDirectory(RemotePath, LocalPath);
        }
    }
}
