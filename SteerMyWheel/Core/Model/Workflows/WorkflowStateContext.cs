using Microsoft.Extensions.Logging;
using SteerMyWheel.Core.Model.Workflows.CommandExecution;
using SteerMyWheel.Domain.Model.Workflow;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows
{
    public class WorkflowStateContext : BaseWorkflowContext
    {
        private ILogger<WorkflowStateContext> _logger;

        private bool Success = false;

        private EventHandler SuccessUpdated;

        
        public WorkflowStateContext(ILoggerFactory loggerFactory) :base(loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<WorkflowStateContext>();
            setState(new WorkflowInitialState());
        }

        public void UpdateSuccess(bool success)
        {
            Success &= success;
        }

        protected virtual void OnSuccessUpdated(object sender,EventArgs e) 
        { 
            SuccessUpdated?.Invoke(this, e);
        }
    }
}
