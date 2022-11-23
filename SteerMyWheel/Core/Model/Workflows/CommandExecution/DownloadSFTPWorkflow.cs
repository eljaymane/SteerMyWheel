using SteerMyWheel.Domain.Model.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.CommandExecution
{
    public class DownloadSFTPWorkflow : BaseWorkflow
    {
        public DownloadSFTPWorkflow(string name, string description, DateTime executionDate, BaseWorkflow next, BaseWorkflow previous) : base(name, description, executionDate, next, previous)
        {
        }

        public override bool CanExecute()
        {
            return true;
        }

        public override Task Execute()
        {
            throw new NotImplementedException();
        }

        public override Task ExecuteAsync()
        {
            throw new NotImplementedException();
        }
    }
}
