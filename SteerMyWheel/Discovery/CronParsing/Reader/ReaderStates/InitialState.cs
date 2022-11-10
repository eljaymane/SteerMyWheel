using SteerMyWheel.CronParsing.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteerMyWheel.Reader.ReaderStates
{
    public class InitialState : IState
    {
        public Host remoteHost { get; }
        public InitialState(Host _host)
        {
            this.remoteHost = _host;
        }
       
        public async void handle(ReaderStateContext context)
        {
            context.currentHostName = this.remoteHost.Name;
            await context.Writer.WriteAsync(remoteHost);
        }
    }
}
