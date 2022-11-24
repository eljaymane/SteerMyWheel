using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.CronReading
{
    public interface IReaderState
    {
        Task handle(ReaderStateContext context);
    }
}
