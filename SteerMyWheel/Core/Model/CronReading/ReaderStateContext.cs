using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SteerMyWheel.Core.Model.CronReading.Exceptions;
using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Infrastracture.Connectivity.Repositories;
using System;

namespace SteerMyWheel.Core.Model.CronReading
{
    /// <summary>
    /// Represents the context of a cron reading process. It's state is updated after parsing every line of the cron file using the CronParser class
    /// </summary>
    public class ReaderStateContext : IDisposable
    {
        private readonly ILogger<ReaderStateContext> _logger;
        /// <summary>
        /// Raised whenever the state of the actual context changes.
        /// </summary>
        public EventHandler StateChanged;
        /// <summary>
        /// Represents the current state of the cron reading context.
        /// </summary>
        public IReaderState currentState;
        /// <summary>
        /// A global graph repository of all the entities that inherit from BaseEntity.
        /// </summary>
        public GlobalEntityRepository _DAO { get; set; }
        /// <summary>
        /// The current role to be attributed to the newly read scrit executions.
        /// </summary>
        public string currentRole { get; set; }
        /// <summary>
        /// The current host name to which the newly read script executions will be linked in the graph database.
        /// </summary>
        public string currentHostName { get; set; }

        public ReaderStateContext() {}
        public ReaderStateContext(ILogger<ReaderStateContext> logger, GlobalEntityRepository DAO)
        {
            _logger = logger;
            _DAO = DAO;
        }
        /// <summary>
        /// Initializes the actual context by setting it's state to the initial state. 
        /// As an effect, the current host name is updated.
        /// </summary>
        /// <param name="host"></param>
        public void Initialize(RemoteHost host)
        {
            setState(new InitialReaderState(host));
            _logger.LogInformation("[{time}] Initializing => Host : {hostname}", DateTime.UtcNow, host.Name);
        }
        /// <summary>
        /// Handler of the StateChanged event. 
        /// Logs the new state and calls the handle method of the state.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void onStateChanged(EventArgs e)
        {
            _logger.LogInformation("[{time}] stateChanged => {newState}", DateTime.UtcNow, currentState.GetType().Name);
            currentState.handle(this).Wait();
        }
        /// <summary>
        /// Updates the state of the actual context.
        /// </summary>
        /// <param name="state"></param>
        /// <exception cref="ReaderStateContextNotInitializedException"></exception>
        public void setState(IReaderState state)
        {
            if (currentHostName.IsNullOrEmpty() && state.GetType() == typeof(InitialReaderState)) throw new ReaderStateContextNotInitializedException();
            currentState = state;
            onStateChanged(EventArgs.Empty);
        }
        /// <summary>
        /// Free the ressource used by the actual context.
        /// </summary>

        public void Dispose()
        {

        }
    }
}
