using SteerMyWheel.Infrastracture.Connectivity.ClientProviders;
using System;

namespace SteerMyWheel.Core.Model.Workflows.Abstractions
{
    public abstract class BaseSSHWorkflow : BaseWorkflow
    {
        public SSHClient _sshClient;
        protected BaseSSHWorkflow(string name, string description, DateTime executionDate, BaseWorkflow next, BaseWorkflow previous) : base(name, description, executionDate, next, previous)
        {
        }

        public void setSSHClient(SSHClient sshCLient)
        {
            _sshClient = sshCLient;
        }



    }
}
