using SteerMyWheel.Core.Model.Entities;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.CronReading
{
    /// <summary>
    /// Represents a state in which the context create a new ScriptExecution and stores it in a graph database.
    /// </summary>
    public class NewScriptReaderState : IReaderState
    {
        public ScriptExecution newScript { get; set; }
        public NewScriptReaderState(ScriptExecution script)
        {
            newScript = script;
        }
        /// <summary>
        /// Creates the newly parsed script execution and links it to the RemoteHost referenced in the given context
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task handle(ReaderStateContext context)
        {
            newScript.Role = context.currentRole;
            context._DAO.ScriptExecutionRepository.CreateAndMatch(newScript, context.currentHostName);
            return Task.CompletedTask;
        }
    }
}
