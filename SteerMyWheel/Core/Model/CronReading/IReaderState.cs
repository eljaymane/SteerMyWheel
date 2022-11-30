using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.CronReading
{
    /// <summary>
    /// Contract of the states of a cron reader context.
    /// </summary>
    public interface IReaderState
    {
        /// <summary>
        /// Handle is called after every state update.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task handle(ReaderStateContext context);
    }
}
