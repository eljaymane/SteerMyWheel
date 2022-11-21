using Microsoft.Extensions.Logging;
using SteerMyWheel.Core.Connectivity.ClientProviders;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SteerMyWheel.Domain.Model.Workflow
{
    public abstract class BaseWorkflowContext
    {
        public IWorkflowState State { get; set; }
        public BaseWorkflow Workflow { get; set; }
        public CancellationToken CancellationToken { get; set; }

        public EventHandler StateChanged;
        public BaseWorkflowContext(BaseWorkflow workflow)
        {
            Workflow = workflow;
        }

        public void setState(IWorkflowState state)
        {
            State = state;
            OnStateChanged(new EventArgs());
        }

        protected virtual async void OnStateChanged(EventArgs e)
        {
            EventHandler handler = StateChanged;
            handler?.Invoke(this, e);
            await State.HandleAsync(this);

        }


    }
}