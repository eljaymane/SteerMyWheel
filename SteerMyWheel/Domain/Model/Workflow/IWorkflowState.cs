using System.Threading.Tasks;
using SteerMyWheel.Core.Model.Entities;

namespace SteerMyWheel.Domain.Model.Workflow
{
    public interface IWorkflowState
    {
        Task HandleAsync(WorkflowContext context);

    }
}