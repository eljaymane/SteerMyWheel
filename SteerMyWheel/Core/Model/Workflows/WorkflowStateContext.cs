using Microsoft.Extensions.Logging;
using SteerMyWheel.Core.Model.Workflows.States;
using System;

namespace SteerMyWheel.Core.Model.Workflows
{
    /// <summary>
    /// A context that represents a workflow execution environment
    /// </summary>
    public class WorkflowStateContext : BaseWorkflowContext
    {
        private bool Success = true;
        private EventHandler SuccessUpdated;


        public WorkflowStateContext(ILogger logger, string name) : base(logger, name) { }
        /// <summary>
        /// Determines if there is still something as a part of the workflow or not
        /// </summary>
        /// <returns>False : if next workflow is null , True if not</returns>
        public bool HasNext()
        {
            return Workflow.Next == null ? false : true;
        }
        /// <summary>
        /// Updates the success property.
        /// </summary>
        /// <param name="success"></param>
        public void UpdateSuccess(bool success)
        {
            Success &= success;
        }
        /// <summary>
        /// Method that handles SuccessUpdated event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnSuccessUpdated(object sender, EventArgs e)
        {
            SuccessUpdated?.Invoke(this, e);
        }
    }
}
