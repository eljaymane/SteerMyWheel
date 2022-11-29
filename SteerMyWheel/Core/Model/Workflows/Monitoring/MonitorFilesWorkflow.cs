﻿using SteerMyWheel.Core.Model.Workflows.Abstractions;
using SteerMyWheel.Core.Model.Workflows.Monitoring.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.Monitoring
{
    public class MonitorFilesWorkflow : BaseMonitoringWorkflow
    {
        private IEnumerable<string> _Paths = null;

        private EventHandler OnFileIsPresent;
        private EventHandler OnAllFilesArePresent;
        public MonitorFilesWorkflow(string[] Paths,string name, string description, DateTime executionDate, BaseWorkflow next, BaseWorkflow previous) : base(name, description, executionDate, next, previous)
        {
            _Paths = Paths;
        }

        public override bool CanExecute()
        {
            throw new NotImplementedException();
        }

        public bool CanGoNext()
        {
            return _CanGoNext;
        }

        public override Task Execute(BaseWorkflowContext context)
        {
            ExecuteAsync().Wait();
            return Task.CompletedTask;
        }

        public override Task ExecuteAsync()
        {
            MonitorAsync().Wait();
            return Task.CompletedTask;
        }

        public override Task MonitorAsync()
        {
            foreach (var path in _Paths)
            {
                var tmp= path.Split('/');
                var file = tmp[tmp.Length- 1];
                var directory = path.Replace(file, "");
                
                if (Directory.Exists(directory.ToString())) new Thread(() =>
                {
                    while (!File.Exists(path)) { Thread.Sleep(10000); }
                    onFileIsPresent(new FileIsPresentEventArgs(path));

                }).Start();
                else throw new DirectoryNotFoundException();
            }
            return Task.CompletedTask;
        }

        private protected void onAllFilesArePresent()
        {
            OnAllFilesArePresent?.Invoke(this, System.EventArgs.Empty);
            _CanGoNext = true;
        }

        private protected void onFileIsPresent(FileIsPresentEventArgs e)
        {
            OnFileIsPresent?.Invoke(this,e);
            _Paths = _Paths.Where(x => x != e.path).ToList();
            if (_Paths.Count() == 0) onAllFilesArePresent();

        }

        
    }
}
