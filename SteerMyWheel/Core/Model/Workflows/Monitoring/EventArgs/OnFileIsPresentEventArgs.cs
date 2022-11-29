using System;

namespace SteerMyWheel.Core.Model.Workflows.Monitoring.Events
{
    internal class OnFileIsPresentEventArgs : EventArgs
    {
        private string path;

        public OnFileIsPresentEventArgs(string path)
        {
            this.path = path;
        }
    }
}