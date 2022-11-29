using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.Conditioned
{
    public class FileConditionedWorkflow : BaseWorkflow
    {
        private string FilePath;

        public FileConditionedWorkflow(string path,string name, string description, DateTime executionDate, BaseWorkflow next, BaseWorkflow previous) : base(name, description, executionDate, next, previous)
        {
            FilePath = path;
        }

        public override bool CanExecute()
        {
            return File.Exists(FilePath);
        }

        public override Task Execute(BaseWorkflowContext context)
        {
            if(CanExecute()) return Task.CompletedTask;
            return Task.FromCanceled(CancellationToken.None);
        }

        public override Task ExecuteAsync()
        {
            throw new NotImplementedException();
        }
    }
}
