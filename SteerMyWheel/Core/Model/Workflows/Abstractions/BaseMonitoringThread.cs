using System;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.Abstractions
{
    public abstract class BaseMonitoringThread
    {
        public delegate void OnThreadDone();

        public virtual Task Run(object callback, BaseWorkflowContext context)
        {
            context._ManualResetEvent.WaitOne();
            Action completeAction = (Action)callback;
            completeAction.Invoke();
            context._ManualResetEvent.Set();
            return Task.CompletedTask;
        }
    }
}
