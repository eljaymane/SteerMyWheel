using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.CronReading
{
    /// <summary>
    /// The state in which the reader context updates the current role.
    /// </summary>
    public class NewRoleReaderState : IReaderState
    {
        /// <summary>
        /// The new role parsed from the cron file.
        /// </summary>
        private string scriptRole { get; }
        public NewRoleReaderState(string role)
        {
            scriptRole = role;
        }
        /// <summary>
        /// Updates the current role to be attributed to the next read script executions.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task handle(ReaderStateContext context)
        {
            context.currentRole = scriptRole;
            return Task.CompletedTask;
        }
    }
}
