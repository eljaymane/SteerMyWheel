using System.Threading.Tasks;

namespace SteerMyWheel.Domain.Model.Workflow
{
    public interface IWorkflowState
    {
        Task HandleAsync(BaseWorkflowContext context);

    }
}