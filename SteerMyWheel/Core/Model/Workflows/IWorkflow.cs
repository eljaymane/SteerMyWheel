using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows
{
    public interface IWorkflow
    {
        Task Execute(BaseWorkflowContext context);
        Task ExecuteAsync(BaseWorkflowContext context);
    }
}
