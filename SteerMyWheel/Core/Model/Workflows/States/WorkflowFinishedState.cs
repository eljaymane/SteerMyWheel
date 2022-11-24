using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.States
{
    public class WorkflowFinishedState : IWorkflowState
    {
        private bool success { get; set; }

        public WorkflowFinishedState(bool success)
        {
            this.success = success;
        }
        public Task HandleAsync(BaseWorkflowContext context)
        {
            ((WorkflowStateContext)context).UpdateSuccess(success);
            return Task.CompletedTask;
        }
    }
}
