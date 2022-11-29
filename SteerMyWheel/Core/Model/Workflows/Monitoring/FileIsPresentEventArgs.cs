using System.Collections.Generic;

namespace SteerMyWheel.Core.Model.Workflows.Monitoring
{
    internal class FileIsPresentEventArgs : System.EventArgs
    {
        public string path;

        public FileIsPresentEventArgs(string path)
        {
            this.path = path;
        }
    }
}