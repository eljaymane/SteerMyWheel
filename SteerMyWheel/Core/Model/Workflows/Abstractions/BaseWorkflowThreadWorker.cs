using SteerMyWheel.Core.Model.WorkersQueue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.Abstractions
{
    public abstract class BaseThread
    {
        public Thread worker;
        public ManualResetEvent ManualResetEvent;
        

        
    }
}
