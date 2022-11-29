using SteerMyWheel.Core.Model.Workflows.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.States
{
    public class MonitoringWorkflowState : IWorkflowState
    {
        public Task HandleAsync(BaseWorkflowContext context)
        {
            context._ManualResetEvent.WaitOne();
            var workflow = (BaseMonitoringWorkflow)context.Workflow;
            workflow.ExecuteAsync(context);
            var waitingThread = new Thread(() =>
            {
               
                while (!workflow._CanGoNext) { Thread.Sleep(10000); }
                context.GoNext();
            });
            waitingThread.Start();
            context._ManualResetEvent.Set();
            return Task.CompletedTask;
        }
    }
}
