using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.WorkersQueue
{
    public class WorkersQueue<T> : IQueue<T> where T : BaseWorker
    {
        private SemaphoreSlim semaphore;
        private readonly Queue<T> _queue;
        private readonly ILogger<IQueuable> _logger;
        public event EventHandler WorkItemAdded;
        public event EventHandler AllWorkItemProcessed;

        public WorkersQueue(ILogger<IQueuable> logger) : this(degreesOfParallelism: 1)
        {
            _queue = new Queue<T>(100);
            _logger = logger;
        }

        public WorkersQueue(int degreesOfParallelism)
        {
            semaphore = new SemaphoreSlim(1);
        }
        protected virtual void OnWorkItemAdded(EventArgs e)
        {
            EventHandler handler = WorkItemAdded;
            handler?.Invoke(this, e);
            _logger.LogInformation("[{time}] New worker added to queue !", DateTime.UtcNow);
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(120)).Token;
            //this.DeqeueAllAsync(cancellationToken);
            //semaphore.WaitAsync().Wait();
            //try
            //{
            //    _queue.Dequeue().doWork().Wait();
            //}
            //finally
            //{
            //    semaphore.Release();
            //}

        }

        protected virtual void OnAllWorkItemProcessed(EventArgs e)
        {
            EventHandler handler = AllWorkItemProcessed;
            handler?.Invoke(this, e);
        }

        public async Task Enqueue(T workItem)
        {
            if (workItem is null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }
            //workItem.setLogger(_loggerFactory.CreateLogger<T>());
            await semaphore.WaitAsync();
            try
            {
                _queue.Enqueue(workItem);
            }
            finally
            {
                semaphore.Release();
                OnWorkItemAdded(new EventArgs());
            }


        }

        public async Task DeqeueAllAsync(CancellationToken cancellationToken)
        {
            var index = 1;
            var count = _queue.Count;
            _logger.LogInformation("[{time}] Started processing queue items ... ", DateTime.UtcNow);
            while (!cancellationToken.IsCancellationRequested && _queue.Count > 0)
            {
                await semaphore.WaitAsync();
                try
                {
                    _logger.LogInformation("[{time}] Processing worker {index}/{count}...", DateTime.UtcNow, index, count);
                    await _queue.Dequeue().doWork();

                }
                finally
                {
                    index++;
                    semaphore.Release();

                }



            }
        }


    }
}
