using System.Threading.Tasks;

namespace SteerMyWheel.TaskQueue
{
    public interface IQueuable
    {
        Task doWork();
    }
}