using SteerMyWheel.Core.Model.Entities;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.CronReading
{
    public class NewScriptReaderState : IReaderState
    {
        public ScriptExecution newScript { get; set; }
        public NewScriptReaderState(ScriptExecution script)
        {
            newScript = script;
        }

        public Task handle(ReaderStateContext context)
        {
            newScript.Role = context.currentRole;
            context._DAO.ScriptExecutionRepository.CreateAndMatch(newScript,context.currentHostName);
            return Task.CompletedTask;
        }
    }
}
