using SteerMyWheel.Writer;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteerMyWheel.CronParsing.Model
{
    public class Script : IWritable
    {
        public string role { get; set; }
        public string cron { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        public string execCommand { get; set; }
        public bool enabled { get; set; }

        public Script(string role, string cron, string name, string path, string execCommand, bool enabled)
        {
            this.role = role;
            this.cron = cron;
            this.name = name;
            this.path = path;
            this.execCommand = execCommand;
            this.enabled = enabled;
        }

        public string CreateQuery()
        {
            return "CREATE(s:Script {Role:'" + role + "'," + "Cron:'" + cron + "'," + "Name:'" + name + "'," + "Path:'" + path + "'," + "ExecCommand:'" + execCommand + "'," + "Enabled:'" + enabled + "'});";
        }
    }
}
