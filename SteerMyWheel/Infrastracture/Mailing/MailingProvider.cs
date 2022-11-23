using Microsoft.Extensions.Logging;
using SteerMyWheel.Domain.Connectivity.ClientProvider;
using System;
using System.ComponentModel;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Infrastracture.Mailing
{
    public class MailingClientProvider : BaseClientProvider<SmtpClient>
    {
        private SmtpClient _client;
        private ILogger<MailingClientProvider> _logger;

        public MailingClientProvider(ILogger<MailingClientProvider> logger)
        {
            
            _logger = logger;
        }

        public async Task SendAsync(MailMessage message)
        {
            _client.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
            await _client.SendMailAsync(message);       
        }

        private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {

        }

        public override Task Connect()
        {
            throw new NotImplementedException();
        }

        public Task Connect(string host)
        {
            _client = new SmtpClient(host);
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public override SmtpClient GetConnection()
        {
            return _client;
        }
    }
}
