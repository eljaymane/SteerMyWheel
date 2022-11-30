using SteerMyWheel.Core.Model.Enums;

namespace SteerMyWheel.Core.Model.Entities
{
    /// <summary>
    /// Represents a remote host in which script executions are executed, or a task should be performed.
    /// </summary>
    public class RemoteHost : BaseEntity<string>
    {
        public string Name { get; set; }
        public string RemoteIP { get; set; }
        public int SSHPort { get; set; }
        public string SSHUsername { get; set; }
        public string SSHPassword { get; set; }
        /// <summary>
        /// Connection method used by SSH. Default : username/password, SSL : Key based authentication.
        /// </summary>
        public SSHConnectionMethod ConnectionMethod { get; set; } = SSHConnectionMethod.DEFAULT;

        public RemoteHost()
        {

        }

        public RemoteHost(string name, string remoteHost, int sSHPort, string sSHUsername, SSHConnectionMethod connectionMethod)
        {
            Name = name;
            RemoteIP = remoteHost;
            SSHPort = sSHPort;
            SSHUsername = sSHUsername;
            ConnectionMethod = connectionMethod;
        }

        public RemoteHost(string name, string remoteHost, int sSHPort, string sSHUsername, string sSHPassword)
        {
            Name = name;
            RemoteIP = remoteHost;
            SSHPort = sSHPort;
            SSHUsername = sSHUsername;
            SSHPassword = sSHPassword;
        }

        public override bool Equals(BaseEntity<string> other)
        {
            return RemoteIP == ((RemoteHost)other).RemoteIP;
        }

        public override string GetID()
        {
            return RemoteIP;
        }
    }
}
