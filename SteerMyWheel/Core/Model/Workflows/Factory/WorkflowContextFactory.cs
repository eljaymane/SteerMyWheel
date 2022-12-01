using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;

namespace SteerMyWheel.Core.Model.Workflows.Factory
{
    /// <summary>
    /// A singleton service that generates and initializes a workflow context through CreateContext
    /// </summary>
    public class WorkflowContextFactory
    {
        /// <summary>
        /// Instance of this singleton : this
        /// </summary>
        private static WorkflowContextFactory instance;

        private ILoggerFactory _loggerFactory;

        public WorkflowContextFactory(ILoggerFactory loggerFactory)
        {
            instance = this;
            _loggerFactory = loggerFactory;
        }
        /// <summary>
        /// Generates a context that contains the workflows in the given queue in the same order they have been enqued.
        /// </summary>
        /// <param name="workflows">Workflows queue.</param>
        /// <param name="Name">Name of the context to be generated.</param>
        /// <returns></returns>
        public WorkflowStateContext CreateContext(Queue<BaseWorkflow> workflows, string Name)
        {
            WorkflowStateContext context = new WorkflowStateContext(_loggerFactory.CreateLogger<BaseWorkflowContext>(), Name);
            var WorkflowsTree = CreateWorkflowTree(workflows);
            context.Initialize(WorkflowsTree, CancellationToken.None);
            return context;
        }

        /// <summary>
        /// Returns the instance of the singleton.
        /// </summary>
        /// <returns></returns>

        public WorkflowContextFactory GetInstance()
        {
            return instance;
        }
        /// <summary>
        /// Recursively creates the workflow tree that will be consumed by the generared context, from a given workflow queue.
        /// </summary>
        /// <param name="workflows">The workflow queue</param>
        /// <returns></returns>
        private BaseWorkflow CreateWorkflowTree(Queue<BaseWorkflow> workflows)
        {
            var workflow = workflows.Dequeue();
            workflow._logger = _loggerFactory.CreateLogger<BaseWorkflow>();
            if (workflows.Count > 0) workflow.Next = CreateWorkflowTree(workflows);
            return workflow;
        }
    }
}
