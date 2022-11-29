using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.WorkersQueue
{
    public interface IQueuable
    {
        Task doWork();
    }
}