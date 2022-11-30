using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.CronReading
{
    /// <summary>
    /// Represents a state in which the context ignores the actual parsed line and goes to the next one.
    /// </summary>
    public class IgnoreReaderState : IReaderState
    {
        public Task handle(ReaderStateContext context)
        {
            return Task.CompletedTask;
        }
    }
}
