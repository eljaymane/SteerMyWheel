using SteerMyWheel.Domain.Model.Entity;

namespace SteerMyWheel.Core.Model.Entities
{
    public class ScriptExecution : BaseEntity<string>
    {
        public string Role { get; set; }
        public string Cron { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string ExecCommand { get; set; }
        public bool Enabled { get; set; }

        public ScriptExecution()
        {

        }
        public ScriptExecution(string role, string cron, string name, string path, string execCommand, bool enabled)
        {
            Role = role;
            Cron = cron;
            Name = name;
            Path = path;
            ExecCommand = execCommand;
            Enabled = enabled;
        }

        public override bool Equals(BaseEntity<string> other)
        {
            return ExecCommand == ((ScriptExecution)other).ExecCommand;
        }
    }
}
