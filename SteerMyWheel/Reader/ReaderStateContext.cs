using SteerMyWheel.Reader.ReaderStates;
using SteerMyWheel.Model;
using SteerMyWheel.Writer;
using SteerMyWheel.Writers.Neo4j;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteerMyWheel.Reader
{
    public class ReaderStateContext : IDisposable
    {
        public EventHandler StateChanged;
        public IState currentState;
        public IWriter<IWritable> Writer { get; set; }
        public string currentRole { get; set; }
        public string currentHostName { get; set; }

        public ReaderStateContext(Host host)
        {
            this.Writer = new Neo4jWriter("http://localhost:7474/","neo4j","Supervision!","neo4j",this);
            this.setState(new InitialState(host));
        }
        protected virtual void onStateChanged(EventArgs e)
        {
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
