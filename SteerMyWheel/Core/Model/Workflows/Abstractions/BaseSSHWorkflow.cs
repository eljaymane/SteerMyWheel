using SteerMyWheel.Infrastracture.Connectivity.ClientProviders;
using System;

namespace SteerMyWheel.Core.Model.Workflows.Abstractions
{
    /// <summary>
    /// The abstract base class of a workflow that relies to SSH
    /// </summary>
    public abstract class BaseSSHWorkflow : BaseWorkflow
    {
        /// <summary>
        /// The ssh client that will perform operations
        /// </summary>
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
