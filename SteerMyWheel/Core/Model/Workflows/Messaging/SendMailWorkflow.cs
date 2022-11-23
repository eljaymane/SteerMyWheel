using SteerMyWheel.Domain.Model.Workflow;
using SteerMyWheel.Infrastracture.Mailing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.Messaging
{
    public class SendMailWorkflow : BaseWorkflow
    {
        public MailMessage message { get; set; }
        private MailingClientProvider _client;

        public SendMailWorkflow(string name, string description, DateTime executionDate, BaseWorkflow next, BaseWorkflow previous) : base(name, description, executionDate, next, previous)
        {
        }

        public override bool CanExecute()
        {
            return true;
        }

        public override Task Execute()
        {
            _client.SendAsync(message).Wait();
            return Task.CompletedTask;
        }

        public async override Task ExecuteAsync()
        {
            await _client.SendAsync(message);
        }
    }
}
