using System.Threading.Tasks;

namespace SteerMyWheel.Domain.Model.WorkerQueue
{
    public interface IQueuable
    {
        Task doWork();
    }
}