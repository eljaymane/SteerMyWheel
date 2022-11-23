using Microsoft.Extensions.Logging;
using SteerMyWheel.Core.Model.Workflows.CommandExecution;
using SteerMyWheel.Domain.Model.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows
{
    public class WorkflowStateContext : BaseWorkflowContext
    {
        private ILogger<WorkflowStateContext> _logger;
        
        public WorkflowStateContext(ILoggerFactory loggerFactory) :base(loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<WorkflowStateContext>();
            setState(new WorkflowInitialState());
        }
    }
}
