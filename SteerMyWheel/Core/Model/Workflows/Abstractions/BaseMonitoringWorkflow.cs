using System;
using System.Threading;
using System.Threading.Tasks;
using static SteerMyWheel.Core.Model.Workflows.Abstractions.AbstractMonitorFilesWorkflow;

namespace SteerMyWheel.Core.Model.Workflows.Abstractions
{
    /// <summary>
    /// Abstract base class of a workflow that is related to monitoring 
    /// </summary>
    public abstract class BaseMonitoringWorkflow : BaseWorkflow
    {
        /// <summary>
        /// The thread that executes the monitoring function
        /// </summary>
        public Thread MonitoringThread;
        /// <summary>
        /// Determines whether the monitoring process has finished or not
        /// </summary>

        public bool _CanGoNext = false;
        public BaseMonitoringWorkflow(string name, string description, DateTime executionDate, BaseWorkflow next, BaseWorkflow previous) : base(name, description, executionDate, next, previous)
        {

        }

        public abstract override bool CanExecute();


        public abstract override Task Execute(BaseWorkflowContext context);

        public abstract override Task ExecuteAsync(BaseWorkflowContext context);
       
    }
}
