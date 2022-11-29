using Microsoft.Extensions.Logging;
using Neo4jClient.Cypher;
using SteerMyWheel.Core.Model.Workflows.CommandExecution;
using SteerMyWheel.Core.Model.Workflows.States;
using System;

namespace SteerMyWheel.Core.Model.Workflows
{
    public class WorkflowStateContext : BaseWorkflowContext
    {
        private ILogger<WorkflowStateContext> _logger;

        private bool Success = true;

        private EventHandler SuccessUpdated;

        
        public WorkflowStateContext(ILoggerFactory loggerFactory) :base(loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<WorkflowStateContext>();
            setState(new WorkflowInitialState());
        }

        public bool HasNext()
        {
            return Workflow.Next == null ? false : true;
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
