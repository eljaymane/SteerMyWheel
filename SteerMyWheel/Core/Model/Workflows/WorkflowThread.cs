using Microsoft.Extensions.Logging;
using System.Threading;

namespace SteerMyWheel.Core.Model.Workflows
{
    public class WorkflowThread
    {
        public ManualResetEvent ManualResetEvent { get; set; }
        private ILogger<WorkflowThread> _logger;
        public WorkflowStateContext _context;

        public WorkflowThread(ILogger<WorkflowThread> logger, WorkflowStateContext context)
        {
            _logger= logger;
            _context = context;
            ManualResetEvent = new ManualResetEvent(false);
        }
    }
}