using SteerMyWheel.Domain.Model.Workflow;
using SteerMyWheel.Infrastracture.Mailing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.CommandExecution
{
    public class SendMailWorkflow : BaseWorkflow
    {
    
        public MailingClientProvider _client { get; set; }
        private MailMessage _message { get; set; }
        
        public SendMailWorkflow(string name,string description,DateTime executionDate,MailMessage message): 
            base(name,description,executionDate,null,null)
        {
            _message = message;
        }
        public override bool CanExecute()
        {
            return true;
        }

        public override Task Execute()
        {
            _client.SendAsync(_message).Wait();
            return Task.CompletedTask;
        }

        public override async Task ExecuteAsync()
        {
            await _client.SendAsync(_message);
         
        }
    }
}
