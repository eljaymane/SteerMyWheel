using SteerMyWheel.Reader.ReaderStates;
using SteerMyWheel.CronParsing;
using SteerMyWheel.Writer;
using System;
using System.Collections.Generic;
using System.Text;
using Neo4j.Driver;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SteerMyWheel.CronParsing.Model;
using SteerMyWheel.CronParsing.Writers.Neo4j;

namespace SteerMyWheel.Reader
{
    public class ReaderStateContext : IDisposable
    {
        public ILoggerFactory _loggerFactory;
        private readonly ILogger<ReaderStateContext> _logger;
        public EventHandler StateChanged;
        public IState currentState;
        public IWriter<IWritable> Writer { get; set; }
        public string currentRole { get; set; }
        public string currentHostName { get; set; }

        public ReaderStateContext(Host host,ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<ReaderStateContext>();
            _logger.LogInformation("[{time}] ( ReaderContext ) ReaderContextInitializing => Host : {hostname}", DateTime.UtcNow,host.Name);
            this.Writer = new Neo4jWriter("http://localhost:7474/","neo4j","Supervision!","neo4j",this);
            this.setState(new InitialState(host));
        }
        protected virtual void onStateChanged(EventArgs e)
        {
            _logger.LogInformation("[{time}] ( ReaderContext ) stateChanged => {newState}",DateTime.UtcNow, this.currentState.GetType().Name);
             this.currentState.handle(this);
        }
        public void setState(IState state)
        {
            this.currentState = state;
            this.onStateChanged(EventArgs.Empty);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

    }
}
