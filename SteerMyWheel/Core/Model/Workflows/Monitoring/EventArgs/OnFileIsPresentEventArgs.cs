using System;

namespace SteerMyWheel.Core.Model.Workflows.Monitoring.EventArgs
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