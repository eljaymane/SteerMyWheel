using System.Threading.Tasks;
using SteerMyWheel.Core.Model.CronReading;

namespace SteerMyWheel.Domain.Model.ReaderState
{
    public interface IReaderState
    {
        Task handle(ReaderStateContext context);
    }
}
