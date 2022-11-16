namespace SteerMyWheel.Model
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
            this.Name = name;
            this.RemoteIP = remoteHost;
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
