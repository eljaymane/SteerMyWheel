using SteerMyWheel.CronParsing.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteerMyWheel.Reader.ReaderStates
{
    public class InitialState : IState
    {
        public RemoteHost remoteHost { get; }
        public InitialState(RemoteHost _host)
        {
            this.remoteHost = _host;
        }
       
        public async void handle(ReaderStateContext context)
        {
            context.currentHostName = this.remoteHost.name;
            await context.Writer.WriteAsync(remoteHost);
        }
    }
}
