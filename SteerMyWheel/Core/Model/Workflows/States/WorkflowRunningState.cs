using Microsoft.Extensions.Logging;
using System;
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
            context._ManualResetEvent.Reset();
            while (!context.CancellationToken.IsCancellationRequested && context.Workflow != null)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                if (context.Workflow.ExecutionDate <= DateTime.Now)
                {
                    context.Workflow.Execute(context).Wait();
                    if (context.Workflow.Next == null)
                    {
                        context.setState(new WorkflowFinishedState(true));
                    }
                    else
                    {
                        context._logger.LogInformation($"[{DateTime.UtcNow}] [Workflow : {context.Name}] Next task : {context.Workflow.Name} ");
                        context.Workflow = context.Workflow.Next;
                        context.setState(new WorkflowRunningState());
                    }
                    return Task.CompletedTask;
                }
            }
            context._ManualResetEvent.Set();
            return Task.CompletedTask;
        }
    }
}
