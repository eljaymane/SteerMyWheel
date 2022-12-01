using SteerMyWheel.Infrastracture.Mailing;
using System;
using System.Net.Mail;
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

        public override Task Execute(BaseWorkflowContext context)
        {
            _client.SendAsync(message).Wait();
            return Task.CompletedTask;
        }

        public async override Task ExecuteAsync(BaseWorkflowContext context)
        {
            await _client.SendAsync(message);
        }
    }
}
