using SteerMyWheel.Domain.Model.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows
{
    public class WorkflowInitialState : IWorkflowState
    {
        public Task HandleAsync(BaseWorkflowContext context)
        {
            while (!context.CancellationToken.IsCancellationRequested)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();
                }
                if (context.Workflow.ExecutionDate >= DateTime.Now)
                {
                    context.Workflow.Execute();
                    context.Workflow = context.Workflow.Next;
                    return Task.CompletedTask;
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
            return Task.CompletedTask;
            
        }
    }
}
