using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.Abstractions
{
    public abstract class BaseMonitoringWorkflow : BaseWorkflow
    {
        public Thread MonitoringThread;

        public bool _CanGoNext = false;
        public BaseMonitoringWorkflow(string name, string description, DateTime executionDate, BaseWorkflow next, BaseWorkflow previous) : base(name, description, executionDate, next, previous)
        {
           
        }

        public abstract override bool CanExecute();


        public abstract override Task Execute(BaseWorkflowContext context);

        public abstract override Task ExecuteAsync();

        public abstract Task MonitorAsync();
    }
}
