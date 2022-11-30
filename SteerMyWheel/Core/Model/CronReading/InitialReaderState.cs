using SteerMyWheel.Core.Model.Entities;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.CronReading
{
    /// <summary>
    /// Represents the initial state of the cron reading context.
    /// RemoteHost represents the remote host for which the actual context was created (that contains the script executions)
    /// </summary>
    public class InitialReaderState : IReaderState
    {
        public RemoteHost remoteHost { get; }
        public InitialReaderState(RemoteHost _host)
        {
            remoteHost = _host;
        }
        /// <summary>
        /// Sets the current host name of the given context and stores the remote host in a graph database.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task handle(ReaderStateContext context)
        {
            context.currentHostName = remoteHost.Name;
            context._DAO.RemoteHostRepository.Create(remoteHost);
            return Task.CompletedTask;
        }
    }
}
