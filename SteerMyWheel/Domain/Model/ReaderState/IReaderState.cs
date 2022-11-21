using System.Threading.Tasks;
using SteerMyWheel.Domain.Discovery.CronParsing.ReaderState;

namespace SteerMyWheel.Domain.Model.ReaderState
{
    public interface IState
    {
        Task handle(ReaderStateContext context);
    }
}
