using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Domain.Model.ReaderState;
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

        public async Task handle(ReaderStateContext context)
        {
            await context._writer.WriteScriptExecution(newScript, context.currentHostName);
        }
    }
}
