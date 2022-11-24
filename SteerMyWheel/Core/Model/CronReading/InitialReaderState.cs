using SteerMyWheel.Core.Model.Entities;
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

        public Task handle(ReaderStateContext context)
        {
            context.currentHostName = remoteHost.Name;
            context._DAO.RemoteHostRepository.Create(remoteHost);
            return Task.CompletedTask;  
        }
    }
}
