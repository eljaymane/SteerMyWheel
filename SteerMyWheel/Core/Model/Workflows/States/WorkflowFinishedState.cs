﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.States
{
    public class WorkflowFinishedState : IWorkflowState
    {
        private bool success { get; set; }

        public WorkflowFinishedState(bool success)
        {
            this.success = success;
        }
        public Task HandleAsync(BaseWorkflowContext context)
        {
            context._ManualResetEvent.WaitOne();
            ((WorkflowStateContext)context).UpdateSuccess(success);
            context._ManualResetEvent.Reset();
            return Task.CompletedTask;
        }
    }
}
