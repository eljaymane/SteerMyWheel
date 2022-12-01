using Microsoft.Extensions.Logging;
using SteerMyWheel.Core.Model.Workflows.States;
using System;
using System.Threading;

namespace SteerMyWheel.Core.Model.Workflows
{
    /// <summary>
    /// Base class of a WorkflowContext
    /// </summary>
    public abstract class BaseWorkflowContext : IDisposable
    {
        public ILogger _logger;
        /// <summary>
        /// Represents the actual state of this context
        /// </summary>
        public IWorkflowState State { get; set; }
        /// <summary>
        /// Represents the workflow to be executed in this context
        /// </summary>
        public BaseWorkflow Workflow { get; set; }
        /// <summary>
        /// Cancellation token that can interrupt the workflow execution. Default : None
        /// </summary>
        public CancellationToken CancellationToken { get; set; }
        /// <summary>
        /// Event handler of "StateChanged" Event
        /// </summary>
        public EventHandler StateChanged;
        /// <summary>
        /// Manual reset events used to manually synchronize the threads created by the execution of the workflow
        /// </summary>
        public ManualResetEvent _ManualResetEvent = new ManualResetEvent(true);
        /// <summary>
        /// Name of the context
        /// </summary>
        public string Name { get; set; }
        public BaseWorkflowContext(ILogger logger, string name)
        {
            _logger = logger;
            Name = name;
        }
        /// <summary>
        /// Initializes the context with the workflow to execute and the cancellation token that can interrupt it's execution.
        /// Then it's sets the state to the initial state.
        /// </summary>
        /// <param name="workflow">The workflow to execute</param>
        /// <param name="cancellationToken">The cancellation token</param>
        public void Initialize(BaseWorkflow workflow, CancellationToken cancellationToken)
        {
            Workflow = workflow;
            CancellationToken = cancellationToken;
            State = new WorkflowInitialState();
        }
        /// <summary>
        /// Changes the state of the actual context
        /// </summary>
        /// <param name="state">The new desired state</param>

        public void setState(IWorkflowState state)
        {
            State = state;
            OnStateChanged(new EventArgs());
        }
        /// <summary>
        /// Handler of StateChanged event
        /// </summary>
        /// <param name="e">Event argument</param>

        protected virtual async void OnStateChanged(EventArgs e)
        {
            EventHandler handler = StateChanged;
            handler?.Invoke(this, e);
            _ManualResetEvent.Reset();
            await State.HandleAsync(this);

        }
        /// <summary>
        /// Go to the next task of the actual workflow to be executed.
        /// </summary>
        public virtual void GoNext()
        {
            if (State != null && Workflow.Next != null) { this.Workflow = Workflow.Next; }
            else { this.Workflow = null; }
        }
        /// <summary>
        /// Free the ressources used by this class.
        /// </summary>
        public virtual void Dispose()
        {
            State = null;
            Workflow = null;
            GC.Collect();
        }
    }
}