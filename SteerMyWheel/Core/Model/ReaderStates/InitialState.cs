using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Domain.Discovery.CronParsing.ReaderState;
using SteerMyWheel.Domain.Model.ReaderState;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.ReaderStates
{
    public class InitialState : IState
    {
        public RemoteHost remoteHost { get; }
        public InitialState(RemoteHost _host)
        {
            remoteHost = _host;
        }

        public async Task handle(ReaderStateContext context)
        {
            context.currentHostName = remoteHost.Name;
            await context._writer.WriteHost(remoteHost);
        }
    }
}
