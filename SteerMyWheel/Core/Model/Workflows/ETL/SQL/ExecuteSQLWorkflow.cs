using SteerMyWheel.Core.Model.Workflows.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.ETL.SQL
{
    public class ExecuteSQLWorkflow : BaseSQLWorkflow
    {
        public ExecuteSQLWorkflow(string name, string description, DateTime executionDate, BaseWorkflow next, BaseWorkflow previous) : base(name, description, executionDate, next, previous)
        {

        }
        public override bool CanExecute()
        {
            return true;
        }

        public override Task Execute(BaseWorkflowContext context)
        {
            if (base.ExecuteQueryWithoutResult().Result) return Task.CompletedTask;
            return Task.FromCanceled(CancellationToken.None);
        }

        public override Task ExecuteAsync(BaseWorkflowContext context)
        {
            throw new NotImplementedException();
        }
    }
}
