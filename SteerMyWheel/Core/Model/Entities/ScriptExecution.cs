namespace SteerMyWheel.Core.Model.Entities
{
    /// <summary>
    /// Represent an execution instance of a given script either read from a cron file or created for a workflow purpose.
    /// </summary>
    public class ScriptExecution : BaseEntity<string>
    {
        /// <summary>
        /// Role of the script execution.
        /// </summary>
        public string Role { get; set; }
        /// <summary>
        /// Cron expression representing the frequence of execution of the actual script.
        /// </summary>
        public string Cron { get; set; }
        /// <summary>
        /// Name of the script.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Path of the script executable.
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// The execution command that represents the script execution.
        /// </summary>
        public string ExecCommand { get; set; }
        /// <summary>
        /// False if the script execution is disabled. True if not. 
        /// </summary>
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

        public override string GetID()
        {
            return ExecCommand;
        }
    }
}
