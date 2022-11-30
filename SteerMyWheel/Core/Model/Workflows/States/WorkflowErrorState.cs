using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.States
{
    public class WorkflowErrorState : IWorkflowState
    {
        public string Message { get; set; }
        public Task HandleAsync(BaseWorkflowContext context)
        {
            context._logger.LogError($"[{DateTime.UtcNow}] [Workflow {context.Name}] {Message}");
            context.Dispose();
            return Task.CompletedTask;
        }
    }
}
