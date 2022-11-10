using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SteerMyWheel.TaskQueue
{
    public class WorkQueue<T> where T : BaseWorker
    {
        private readonly Queue<T> _queue;
        private readonly ILoggerFactory _loggerFactory;
        public event EventHandler WorkItemAdded;
        public event EventHandler AllWorkItemProcessed;

        protected virtual void OnWorkItemAdded(EventArgs e)
        {
            EventHandler handler = WorkItemAdded;
            handler?.Invoke(this, e);
        }

        protected virtual void OnAllWorkItemProcessed(EventArgs e)
        {
            EventHandler handler = AllWorkItemProcessed;
            handler?.Invoke(this, e);
        }

        public WorkQueue(ILoggerFactory loggerFactory,int capacity)
        {
            _queue = new Queue<T>(capacity);
            _loggerFactory = loggerFactory;
        }
        public void Enqueue(T workItem)
        {
            if (workItem is null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }
            workItem.setLogger(_loggerFactory.CreateLogger<T>());
            
             _queue.Enqueue(workItem);
            OnWorkItemAdded(new EventArgs());
        }

        public async Task DeqeueAllAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && _queue.Count > 0)
            {
                await _queue.Dequeue().doWork();
            }
        }


    }
}
