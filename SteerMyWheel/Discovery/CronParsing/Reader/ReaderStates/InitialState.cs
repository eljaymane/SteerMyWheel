using SteerMyWheel.Model;
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
            context.currentHostName = this.remoteHost.Name;
            await context._writer.WriteAsync(remoteHost);
        }
    }
}
