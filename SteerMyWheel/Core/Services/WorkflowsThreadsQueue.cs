using Microsoft.Extensions.Logging;
using SteerMyWheel.Configuration;
using SteerMyWheel.Core.Model.Workflows;
using SteerMyWheel.Core.Model.Workflows.States;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Services
{
    /// <summary>
    /// Represents the service on which all workflows are queued and processed
    /// </summary>
    public class WorkflowsThreadsQueue
    {
        /// <summary>
        /// The workflows context queue (encapsulated in WorkflowThread).
        /// </summary>
        private Queue<WorkflowThread> _Queue;
        private ILogger<WorkflowsThreadsQueue> _Logger;

        public WorkflowsThreadsQueue(GlobalConfig _config, ILogger<WorkflowsThreadsQueue> logger)
        {
            _Queue = new Queue<WorkflowThread>(_config.MaxWorfklowQueueCapacity);
            _Logger = logger;
        }
        /// <summary>
        /// Add a WorkflowThread to the queue.
        /// </summary>
        /// <param name="thread">The WorkflowThread to be enqued</param>
        public void Enqueue(WorkflowThread thread)
        {
            _Logger.LogInformation($"[{DateTime.UtcNow}] Workflow {thread._context.Name} added to execution queue !");
            _Queue.Enqueue(thread);
        }
        /// <summary>
        /// Parallel process all the enqueued WorkflowsThreads.
        /// The cancellation token is able to interrupt the processing of a workflow context.
        /// If the execution date is not today, the thread is blocked.
        /// </summary>
        public void Process()
        {
            Parallel.ForEach(_Queue, new ParallelOptions { MaxDegreeOfParallelism = 10, CancellationToken = CancellationToken.None }, e =>
            {
                while (e._context.State.GetType() != typeof(WorkflowFinishedState))
                {
                    if (e._context.Workflow.ExecutionDate <= DateTime.Now)
                    {
                        if (e.ManualResetEvent.Set())
                        {
                            e._context.setState(new WorkflowRunningState());
                        }
                    }
                    else
                    {
                        if (e._context.Workflow.ExecutionDate > DateTime.Now) { e.ManualResetEvent.Reset(); }
                    }
                }
                e._context.Dispose();
            });
        }
    }
}
