using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.Factory
{
    public class WorkflowContextFactory
    {

        private static WorkflowContextFactory instance;

        private ILoggerFactory _loggerFactory;
            
        public WorkflowContextFactory(ILoggerFactory loggerFactory)
        {
            instance = this;
            _loggerFactory = loggerFactory;
        }

        public WorkflowStateContext CreateContext(Queue<BaseWorkflow> workflows, string Name)
        {
            WorkflowStateContext context = new WorkflowStateContext(_loggerFactory);
            var WorkflowsTree = CreateWorkflowTree(workflows);
            context.Initialize(WorkflowsTree, CancellationToken.None);
            return context;
        }

        public WorkflowContextFactory GetInstance()
        {
            return instance;
        }

        private BaseWorkflow CreateWorkflowTree(Queue<BaseWorkflow> workflows)
        {
            var workflow = workflows.Dequeue(); 
            if(workflows.Count > 0) workflow.Next = CreateWorkflowTree(workflows);
            return workflow;
        }
    }
}
