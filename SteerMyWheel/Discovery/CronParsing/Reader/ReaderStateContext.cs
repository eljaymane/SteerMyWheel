using SteerMyWheel.Reader.ReaderStates;
using System;
using Microsoft.Extensions.Logging;
using SteerMyWheel.Model;
using SteerMyWheel.CronParsing.Writers.Neo4j;

namespace SteerMyWheel.Reader
{
    public class ReaderStateContext : IDisposable
    {
        private readonly ILogger<ReaderStateContext> _logger;
        private readonly GlobalConfig _config;
        public EventHandler StateChanged;
        public IState currentState;
        public Neo4jWriter _writer { get; set; }
        public string currentRole { get; set; }
        public string currentHostName { get; set; }

        public ReaderStateContext(ILogger<ReaderStateContext>logger,GlobalConfig config,Neo4jWriter writer)
        {
            _logger = logger;
            _config = config;
            _writer = writer;
            _writer.setContext(this);
        }

        public void Initialize(RemoteHost host)
        {
            
            this.setState(new InitialState(host));
            _logger.LogInformation("[{time}] ( ReaderContext ) Initializing => Host : {hostname}", DateTime.UtcNow, host.Name);
        }
        protected virtual void onStateChanged(EventArgs e)
        {
            _logger.LogInformation("[{time}] ( ReaderContext ) stateChanged => {newState}",DateTime.UtcNow, this.currentState.GetType().Name);
             this.currentState.handle(this).Wait();
        }
        public void setState(IState state)
        {
            this.currentState = state;
            this.onStateChanged(EventArgs.Empty);
        }

        public void Dispose()
        {
            
        }

    }
}
