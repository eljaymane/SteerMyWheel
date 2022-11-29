using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.States
{
    public class WorkflowPausedState : IWorkflowState
    {
        public WorkflowPausedState() { }

        public Task HandleAsync(BaseWorkflowContext context)
        {
            context._ManualResetEvent.Reset();
            return Task.CompletedTask;
        }
    }
}
