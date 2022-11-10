using SteerMyWheel.Writer;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteerMyWheel.CronParsing.Model
{
    public class Host : IWritable
    {
        public string Name { get; set; }
        public string RemoteHost { get; set; }
        public int SSHPort { get; set; }
        public string SSHUsername { get; set; }
        public string SSHPassword { get; set; }

        public Host(string name, string remoteHost, int sSHPort, string sSHUsername, string sSHPassword)
        {
            Name = name;
            RemoteHost = remoteHost;
            SSHPort = sSHPort;
            SSHUsername = sSHUsername;
            SSHPassword = sSHPassword;
        }

        public string CreateQuery()
        {
            return "Name:" + Name + ";RemoteHost:" + RemoteHost + ";SSHPort:" + SSHPort + ";SSHUsername:" + SSHUsername + ";SSHPassword" + SSHPassword + ";";
        }
    }
}
