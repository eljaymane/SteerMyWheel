using SteerMyWheel.Core.Model.Workflows.Abstractions;
using System;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.ETL.SQL
{
    public class GetSQLDataWorkflow : BaseSQLWorkflow
    {


        public GetSQLDataWorkflow(string name, string description, DateTime executionDate, BaseWorkflow next, BaseWorkflow previous) : base(name, description, executionDate, next, previous)
        {

        }
        public override bool CanExecute()
        {
            throw new NotImplementedException();
        }

        public override Task Execute(BaseWorkflowContext context)
        {
            data = base.ExecuteQueryWithResult();
            return Task.CompletedTask;
        }

        public override Task ExecuteAsync()
        {
            throw new NotImplementedException();
        }
    }
}
