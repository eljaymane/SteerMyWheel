using Microsoft.Extensions.Logging;
using SteerMyWheel.Core.Connectivity.ClientProviders;
using SteerMyWheel.Core.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.CommandExecution
{
    public class ScriptExecutionWorkflow : BaseWorkflow
    {
        private ILogger<ScriptExecutionWorkflow> _logger { get; set; }

        private readonly ScriptExecution _scriptExecution;
        private SSHClientProvider _SSHClient { get; set; }
        private RemoteHost _RemoteHost { get; set; }

        public override bool CanExecute()
        {
            return true;
        }

        public ScriptExecutionWorkflow(string name, string description,DateTime executionDate, ScriptExecution scriptExecution) : base(name, description,executionDate, null, null)
        {
            _scriptExecution = scriptExecution;
        }

        public override Task Execute()
        {
            _SSHClient.ConnectSSH(_RemoteHost).Wait();
            if (_SSHClient.ExecuteCmd(_scriptExecution.ExecCommand).Result)
            {
                _logger.LogInformation($"[{DateTime.UtcNow}] Successfully executed a workflow block");

            }
            else
            {
                _logger.LogError($"[{DateTime.UtcNow}] There was a problem executing a workflow block");
            }
            return Task.CompletedTask;



        }

        public override Task ExecuteAsync()
        {
            throw new NotImplementedException();
        }
    }
}
