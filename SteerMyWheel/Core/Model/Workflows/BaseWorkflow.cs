using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows
{
    public abstract class BaseWorkflow : IWorkflow
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public BaseWorkflow Next { get; set; }
        public BaseWorkflow Previous { get; set; }
        public DateTime ExecutionDate { get; set; }

        public ILogger _logger { get; set;}

        public abstract bool CanExecute();


        public BaseWorkflow(string name, string description, DateTime executionDate, BaseWorkflow next, BaseWorkflow previous)
        {
            Id = Guid.NewGuid();
            Name = name;
            ExecutionDate = executionDate;
            Description = description;
            Next = next;
            Previous = previous;
        }

        public abstract Task Execute(BaseWorkflowContext context);
        public abstract Task ExecuteAsync(BaseWorkflowContext context);
    }
}
