using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.States
{  
    /// <summary>
    /// "Finished" state of a workflow. This marks the end of a workflow execution
    /// </summary>
    public class WorkflowFinishedState : IWorkflowState
    {
        /// <summary>
        /// Determines if the workflow execution has been successfull or not.
        /// </summary>
        private bool success { get; set; }
        public WorkflowFinishedState(bool success)
        {
            this.success = success;
        }
        /// <summary>
        /// Logs the end of the workflow execution.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task HandleAsync(BaseWorkflowContext context)
        {
            ((WorkflowStateContext)context).UpdateSuccess(success);
            ((WorkflowStateContext)context)._logger.LogInformation($"[{DateTime.UtcNow}] [Workflow : {context.Name}] Finished execution of the workflow !");
            return Task.CompletedTask;
        }
    }
}
