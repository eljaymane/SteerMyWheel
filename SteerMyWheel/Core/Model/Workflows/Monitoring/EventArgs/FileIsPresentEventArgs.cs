namespace SteerMyWheel.Core.Model.Workflows.Monitoring.EventArgs
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