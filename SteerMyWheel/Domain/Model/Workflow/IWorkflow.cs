using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Domain.Model.Workflow
{
    public interface IWorkflow
    {
        Task Execute();
        Task ExecuteAsync();
    }
}
