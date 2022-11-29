using SteerMyWheel.Configuration;
using SteerMyWheel.Core.Model.Workflows;
using SteerMyWheel.Core.Model.Workflows.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Services
{
    public class WorkflowsThreadsQueue 
    {
        private Queue<WorkflowThread> _Queue;

        public WorkflowsThreadsQueue(GlobalConfig _config)
        {
            _Queue= new Queue<WorkflowThread>(_config.MaxWorfklowQueueCapacity);
        }

        public void Enqueue(WorkflowThread thread)
        {
            _Queue.Enqueue(thread);
        }

        public void Process()
        {
            Parallel.ForEach(_Queue,new ParallelOptions { MaxDegreeOfParallelism = 10, CancellationToken = CancellationToken.None }, e =>
            {
                

                if (e._context.Workflow.ExecutionDate >= DateTime.Now)
                {
                    if (e.ManualResetEvent.Set())
                    {
                        e._context.setState(new WorkflowRunningState());
                    }
                }
                else
                {
                    if (e._context.Workflow.ExecutionDate.Day < DateTime.Today.Day) { e.ManualResetEvent.Reset(); }
                }
            });
        }
    }
}
