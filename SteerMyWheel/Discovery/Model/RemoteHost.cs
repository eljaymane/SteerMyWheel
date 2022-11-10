using SteerMyWheel.Writer;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteerMyWheel.CronParsing.Model
{
    public class RemoteHost : IWritable
    {
        public string name { get; set; }
        public string remoteHost { get; set; }
        public int sshPort { get; set; }
        public string SSHUsername { get; set; }
        public string SSHPassword { get; set; }

        public RemoteHost(string name, string remoteHost, int sSHPort, string sSHUsername, string sSHPassword)
        {
            this.name = name;
            this.remoteHost = remoteHost;
            sshPort = sSHPort;
            SSHUsername = sSHUsername;
            SSHPassword = sSHPassword;
        }

        public string CreateQuery()
        {
            return "Name:" + name + ";RemoteHost:" + remoteHost + ";SSHPort:" + sshPort + ";SSHUsername:" + SSHUsername + ";SSHPassword" + SSHPassword + ";";
        }
    }
}
