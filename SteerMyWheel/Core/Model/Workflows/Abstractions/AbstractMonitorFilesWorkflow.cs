using Microsoft.Extensions.Logging;
using SteerMyWheel.Core.Model.Workflows.Monitoring.EventArgs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.Abstractions
{
    public abstract class AbstractMonitorFilesWorkflow : BaseMonitoringWorkflow
    {
        private IEnumerable<string> _Paths = null;

        private EventHandler OnFileIsPresent;
        private EventHandler OnAllFilesArePresent;

        public delegate bool FileExistsDelegate(string path);
        public delegate bool DirectoryExistsDelegate(string path);
        public AbstractMonitorFilesWorkflow(string[] Paths, string name, string description, DateTime executionDate, BaseWorkflow next, BaseWorkflow previous) : base(name, description, executionDate, next, previous)
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
            ExecuteAsync(context).Wait();
            return Task.CompletedTask;
        }

        public override abstract Task ExecuteAsync(BaseWorkflowContext context);
       
        public override Task MonitorAsync(FileExistsDelegate _FileExists, DirectoryExistsDelegate _DirectoryExists, BaseWorkflowContext context)
        {
            foreach (var path in _Paths)
            {
                var tmp = path.Split('/');
                var file = tmp[tmp.Length - 1];
                var directory = path.Replace(file, "");

                if (_DirectoryExists.Invoke(directory)) new Thread(() =>
                {

                    while (!_FileExists.Invoke(path))
                    {
                        _logger.LogInformation($"[{DateTime.UtcNow}] [Workflow : {context.Name} File {path} not found yet. Awaiting...");
                        Thread.Sleep(10000);
                    }
                    _logger.LogInformation($"[{DateTime.UtcNow}] File {path} arrived !");
                    onFileIsPresent(new FileIsPresentEventArgs(path), context);

                }).Start();
                else throw new DirectoryNotFoundException($"Directory {directory} does not exist !");
            }

            return Task.CompletedTask;
        }
        private protected void onAllFilesArePresent(BaseWorkflowContext context)
        {
            OnAllFilesArePresent?.Invoke(this, EventArgs.Empty);
            _logger.LogInformation($"[{DateTime.UtcNow}] [Workflow : {context.Name}] All files are present, continuing workflow execution...");
            _CanGoNext = true;

        }

        private protected void onFileIsPresent(FileIsPresentEventArgs e, BaseWorkflowContext context)
        {
            OnFileIsPresent?.Invoke(this, e);
            _Paths = _Paths.Where(x => x != e.path).ToList();
            if (_Paths.Count() == 0) onAllFilesArePresent(context);

        }


    }
}
