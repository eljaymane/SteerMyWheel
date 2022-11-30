using SteerMyWheel.Core.Model.Workflows.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.States
{
    /// <summary>
    /// Represents a state in which a given workflow context should perform a monitoring task
    /// </summary>
    public class MonitoringWorkflowState : IWorkflowState
    {
        /// <summary>
        /// Implementation of HandleAsync : Lanches the monitoring task through ExecuteAsync(context), 
        /// then waits for CanGoNext to be true to continue the workflow execution through context.GoNext()
        /// </summary>
        /// <param name="context">Reference to the context that this state represents</param>
        /// <returns></returns>
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
