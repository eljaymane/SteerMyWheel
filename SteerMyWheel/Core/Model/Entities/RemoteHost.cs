using SteerMyWheel.Domain.Model.Entity;

namespace SteerMyWheel.Core.Model.Entities
{
    public class RemoteHost : BaseEntity<string>
    {
        public string Name { get; set; }
        public string RemoteIP { get; set; }
        public int SSHPort { get; set; }
        public string SSHUsername { get; set; }
        public string SSHPassword { get; set; }

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
    }
}
