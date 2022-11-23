using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Domain.Model.ReaderState;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.CronReading
{
    public class InitialReaderState : IReaderState
    {
        public RemoteHost remoteHost { get; }
        public InitialReaderState(RemoteHost _host)
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
