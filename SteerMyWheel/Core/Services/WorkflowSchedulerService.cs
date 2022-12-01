using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Services
{
    public class WorkflowSchedulerService : TaskScheduler
    {
        [ThreadStatic]
        private static bool _CurrentThreadIsProcessingItems;
        private readonly LinkedList<Task> _Tasks = new LinkedList<Task>();
        private readonly int _MaxConcurrencyLevel = 10;
        private int _DelegatesQueuedOrRunning;

        public WorkflowSchedulerService()
        {

        }

        protected override void QueueTask(Task task)
        {
            lock (_Tasks)
            {
                _Tasks.AddLast(task);
                if (_DelegatesQueuedOrRunning < _MaxConcurrencyLevel)
                {
                    ++_DelegatesQueuedOrRunning;
                    NotifyThreadPoolOfPendingWork();
                }
            }
        }

        // Inform the ThreadPool that there's work to be executed for this scheduler.
        private void NotifyThreadPoolOfPendingWork()
        {
            ThreadPool.UnsafeQueueUserWorkItem(_ =>
            {
                // Note that the current thread is now processing work items.
                // This is necessary to enable inlining of tasks into this thread.
                _CurrentThreadIsProcessingItems = true;
                try
                {
                    // Process all available items in the queue.
                    while (true)
                    {
                        Task item;
                        lock (_Tasks)
                        {
                            // When there are no more items to be processed,
                            // note that we're done processing, and get out.
                            if (_Tasks.Count == 0)
                            {
                                --_DelegatesQueuedOrRunning;
                                break;
                            }

                            // Get the next item from the queue
                            item = _Tasks.First.Value;
                            _Tasks.RemoveFirst();
                        }

                        // Execute the task we pulled out of the queue
                        base.TryExecuteTask(item);
                    }
                }
                // We're done processing items on the current thread
                finally { _CurrentThreadIsProcessingItems = false; }
            }, null);
        }

        protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            // If this thread isn't already processing a task, we don't support inlining
            if (!_CurrentThreadIsProcessingItems) return false;

            // If the task was previously queued, remove it from the queue
            if (taskWasPreviouslyQueued)
                // Try to run the task.
                if (TryDequeue(task))
                    return base.TryExecuteTask(task);
                else
                    return false;
            else
                return base.TryExecuteTask(task);
        }

        // Attempt to remove a previously scheduled task from the scheduler.
        protected sealed override bool TryDequeue(Task task)
        {
            lock (_Tasks) return _Tasks.Remove(task);
        }

        protected sealed override IEnumerable<Task> GetScheduledTasks()
        {
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(_Tasks, ref lockTaken);
                if (lockTaken) return _Tasks;
                else throw new NotSupportedException();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_Tasks);
            }
        }
    }

}
