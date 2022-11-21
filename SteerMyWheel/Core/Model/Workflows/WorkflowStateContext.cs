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
        
        public WorkflowStateContext(ExecutionWorkflow workflow) : base(workflow)
        {
            Workflow = workflow;
            setState(new WorkflowInitialState());
        }
    }
}
