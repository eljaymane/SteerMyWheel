using Microsoft.Extensions.Logging;

namespace SteerMyWheel.Infrastracture.Mailing
{
    public class MailingService
    {
        private ILoggerFactory loggerFactory;
        private MailingClientProvider mailingProvider;

        public MailingService(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        private void Initialize(string host)
        {
            mailingProvider = new MailingClientProvider(loggerFactory.CreateLogger<MailingClientProvider>());
            mailingProvider.Connect(host);
        }

        private async void SendAsync(string from, string name, string to, string subject, string text)
        {
            if (mailingProvider == null) return;
            await mailingProvider.SendAsync(new System.Net.Mail.MailMessage(from,to,subject,text));
        }
    }
}
