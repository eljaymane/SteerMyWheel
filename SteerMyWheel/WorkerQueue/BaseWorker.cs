using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.TaskQueue
{
    public abstract class BaseWorker : IQueuable
    {
        public ILogger<BaseWorker> Logger { get; set; }

        public abstract Task doWork();

        public virtual void setLogger(ILogger<BaseWorker> logger)
        {
            Logger = logger;
        }
    }
}
