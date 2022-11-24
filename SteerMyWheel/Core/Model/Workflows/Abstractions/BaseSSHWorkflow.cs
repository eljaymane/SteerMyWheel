﻿using SteerMyWheel.Core.Connectivity.ClientProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.Abstractions
{
    public abstract class BaseSSHWorkflow : BaseWorkflow
    {
        public SSHClientProvider _sshClient;
        protected BaseSSHWorkflow(string name, string description, DateTime executionDate, BaseWorkflow next, BaseWorkflow previous) : base(name, description, executionDate, next, previous)
        {
        }

        public void setSSHClient(SSHClientProvider sshCLient)
        {
            _sshClient = sshCLient;
        }



    }
}
