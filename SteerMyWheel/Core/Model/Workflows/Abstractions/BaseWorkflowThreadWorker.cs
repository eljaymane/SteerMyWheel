using System.Threading;

namespace SteerMyWheel.Core.Model.Workflows.Abstractions
{
    public abstract class BaseThread
    {
        public Thread worker;
        public ManualResetEvent ManualResetEvent;



    }
}
