using SteerMyWheel.CronParsing.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Reader.ReaderStates
{
    public class InitialState : IState
    {
        public RemoteHost remoteHost { get; }
        public InitialState(RemoteHost _host)
        {
            this.remoteHost = _host;
        }
       
        public async Task handle(ReaderStateContext context)
        {
            context.currentHostName = this.remoteHost.name;
            await context._writer.WriteAsync(remoteHost);
        }
    }
}
