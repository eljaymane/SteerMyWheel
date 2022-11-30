using Microsoft.Extensions.Logging;
using SteerMyWheel.Core.Model.Workflows.Abstractions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.Monitoring
{
    public class MonitorLocalFilesWorkflow : AbstractMonitorFilesWorkflow
    {
        public MonitorLocalFilesWorkflow(string[] Paths, string name, string description, DateTime executionDate, BaseWorkflow next, BaseWorkflow previous) : base(Paths, name, description, executionDate, next, previous)
        {

        }
        public Task MonitorAsync(BaseWorkflowContext context)
        {
            base.MonitorAsync(File.Exists, Directory.Exists, context).Wait();
            return Task.CompletedTask;
        }

        public override Task ExecuteAsync(BaseWorkflowContext context)
        {
            try
            {
                MonitorAsync(context).Wait();
            }
            catch (DirectoryNotFoundException e)
            {
                _logger.LogError($"[{DateTime.UtcNow}] [Workflow : {context.Name}] Directory not found : {e.Message}");
            }

            return Task.CompletedTask;
        }
    }
}
