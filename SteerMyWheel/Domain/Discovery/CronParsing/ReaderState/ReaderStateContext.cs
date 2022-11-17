using System;
using Microsoft.Extensions.Logging;
using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Core.Model.ReaderStates;
using SteerMyWheel.Domain.Model.ReaderState;
using SteerMyWheel.Core.Discovery.Crontab.GraphWriter;

namespace SteerMyWheel.Domain.Discovery.CronParsing.ReaderState
{
    public class ReaderStateContext : IDisposable
    {
        private readonly ILogger<ReaderStateContext> _logger;
        public EventHandler StateChanged;
        public IState currentState;
        public CronGraphWriter _writer { get; set; }
        public string currentRole { get; set; }
        public string currentHostName { get; set; }

        public ReaderStateContext(ILogger<ReaderStateContext> logger, CronGraphWriter writer)
        {
            _logger = logger;
            _writer = writer;
            _writer.setContext(this);
        }

        public void Initialize(RemoteHost host)
        {

            setState(new InitialState(host));
            _logger.LogInformation("[{time}] ( ReaderContext ) Initializing => Host : {hostname}", DateTime.UtcNow, host.Name);
        }
        protected virtual void onStateChanged(EventArgs e)
        {
            _logger.LogInformation("[{time}] ( ReaderContext ) stateChanged => {newState}", DateTime.UtcNow, currentState.GetType().Name);
            currentState.handle(this).Wait();
        }
        public void setState(IState state)
        {
            currentState = state;
            onStateChanged(EventArgs.Empty);
        }

        public void Dispose()
        {

        }

    }
}
