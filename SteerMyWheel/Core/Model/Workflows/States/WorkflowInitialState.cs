using System;
using System.Threading;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.States
{
    public class WorkflowInitialState : IWorkflowState
    {
        public Task HandleAsync(BaseWorkflowContext context)
        {
            var execTime = context.Workflow.ExecutionDate;
            context._ManualResetEvent.Set();
            while (!context.CancellationToken.IsCancellationRequested)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();
                }
                if (execTime.Month == DateTime.Now.Month && execTime.Day == DateTime.Now.Day && execTime.Hour <= DateTime.Now.Hour && execTime.Minute <= DateTime.Now.Minute)
                {
                    context.Workflow.Execute(context);
                    context.Workflow = context.Workflow.Next;
                    return Task.CompletedTask;
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
            context._ManualResetEvent.Reset();
            return Task.CompletedTask;

        }
    }
}
