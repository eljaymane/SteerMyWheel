using System;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SteerMyWheel.Core.Model.CronReading.Exceptions;
using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Infrastracture.Connectivity.Repositories;

namespace SteerMyWheel.Core.Model.CronReading
{
    public class ReaderStateContext : IDisposable
    {
        private readonly ILogger<ReaderStateContext> _logger;
        public EventHandler StateChanged;
        public IReaderState currentState;
        public GlobalEntityRepository _DAO { get; set; }
        public string currentRole { get; set; }
        public string currentHostName { get; set; }

        public ReaderStateContext()
        {

        }
        public ReaderStateContext(ILogger<ReaderStateContext> logger, GlobalEntityRepository DAO)
        {
            _logger = logger;
            _DAO = DAO;
        }

        public void Initialize(RemoteHost host)
        {
            setState(new InitialReaderState(host));
            _logger.LogInformation("[{time}] Initializing => Host : {hostname}", DateTime.UtcNow, host.Name);
        }
        protected virtual void onStateChanged(EventArgs e)
        {
            _logger.LogInformation("[{time}] stateChanged => {newState}", DateTime.UtcNow, currentState.GetType().Name);
            currentState.handle(this).Wait();
        }
        public void setState(IReaderState state)
        {
            if (currentHostName.IsNullOrEmpty() && state.GetType() == typeof(InitialReaderState)) throw new ReaderStateContextNotInitializedException();
            currentState = state;
            onStateChanged(EventArgs.Empty);
        }

        public void Dispose()
        {

        }
    }
}
