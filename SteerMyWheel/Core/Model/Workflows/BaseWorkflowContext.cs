using Microsoft.Extensions.Logging;
using SteerMyWheel.Core.Connectivity.ClientProviders;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows
{
    public abstract class BaseWorkflowContext
    {
        private readonly ILoggerFactory _loggerFactory;
        public IWorkflowState State { get; set; }
        public BaseWorkflow Workflow { get; set; }
        public CancellationToken CancellationToken { get; set; }

        public EventHandler StateChanged;
        public BaseWorkflowContext(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public void Initialize(BaseWorkflow workflow, CancellationToken cancellationToken)
        {
            Workflow = workflow;
            CancellationToken = cancellationToken;
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