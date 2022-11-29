using System.Collections.Generic;

namespace SteerMyWheel.Core.Model.Workflows.Monitoring
{
    internal class FileIsPresentEventArgs : System.EventArgs
    {
        private string path;

        public FileIsPresentEventArgs(string path)
        {
            this.path = path;
        }
    }
}