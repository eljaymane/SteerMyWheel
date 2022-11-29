using Microsoft.Extensions.Logging;
using SteerMyWheel.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.CommandExecution
{
    public class LocalCommandExecutionWorkflow : BaseWorkflow
    {
        private string _ExecCommand { get; set; }
        public LocalCommandExecutionWorkflow(string ExecCommand,string name, string description, DateTime executionDate, BaseWorkflow next, BaseWorkflow previous) : base(name, description, executionDate, next, previous)
        {
            _ExecCommand = ExecCommand;
        }

       
        public override bool CanExecute()
        {
            return true;
        }

        public override Task Execute(BaseWorkflowContext context)
        {
            WinAPI.system(_ExecCommand);
            return Task.CompletedTask;
        }

        public override Task ExecuteAsync(BaseWorkflowContext context)
        {
            _logger.LogInformation($"[{DateTime.UtcNow}] [Workflow : {context.Name}] Executing...");
            WinAPI.system(_ExecCommand);
            return Task.CompletedTask;

        }
    }
}
