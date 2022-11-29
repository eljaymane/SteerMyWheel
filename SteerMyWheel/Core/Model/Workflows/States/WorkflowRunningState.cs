using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.States
{
    public class WorkflowRunningState : IWorkflowState
    {
        public WorkflowRunningState()
        {

        }
        public Task HandleAsync(BaseWorkflowContext context)
        {
            context._ManualResetEvent.Set();
            context._ManualResetEvent.WaitOne();
            while (!context.CancellationToken.IsCancellationRequested && context.Workflow != null)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                if (context.Workflow.ExecutionDate >= DateTime.Now)
                {
                    context.Workflow.Execute(context).Wait();
                    context.Workflow = context.Workflow.Next;
                    context.setState(new WorkflowRunningState());
                    return Task.CompletedTask;
                }
            }
            return Task.CompletedTask;
        }
    }
}
