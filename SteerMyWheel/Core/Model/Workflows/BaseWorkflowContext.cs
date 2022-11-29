using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows
{
    public abstract class BaseWorkflowContext : IDisposable
    {
        private readonly ILoggerFactory _loggerFactory;
        public IWorkflowState State { get; set; }
        public BaseWorkflow Workflow { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public EventHandler StateChanged;
        public ManualResetEvent _ManualResetEvent = new ManualResetEvent(false);
        public string Name { get; set; }    
        public BaseWorkflowContext(ILoggerFactory loggerFactory,string name)
        {
            _loggerFactory = loggerFactory;
            Name = name;
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
            _ManualResetEvent.Reset();
            await State.HandleAsync(this);

        }

        public virtual void GoNext()
        {
            if (State != null && Workflow.Next != null ) { this.Workflow = Workflow.Next; }
        }

        public virtual void Dispose()
        {
            State = null;
            Workflow = null;
            GC.Collect();
        }
    }
}