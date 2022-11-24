using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows
{
    public interface IWorkflowState
    {
        Task HandleAsync(BaseWorkflowContext context);

    }
}