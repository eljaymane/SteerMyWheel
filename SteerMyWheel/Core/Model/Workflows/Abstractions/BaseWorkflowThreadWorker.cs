using SteerMyWheel.Core.Model.WorkersQueue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.Abstractions
{
    public class BaseWorkflowThreadWorker 
    {
        public Thread worker;

        public override Task doWork(ref WorkflowStateContext context)
        {
            context._ManualResetEvent.WaitOne();
            worker = new Thread(() =>
            {
                if(context.HasNext()) context.GoNext();
            });
        }
    }
}
