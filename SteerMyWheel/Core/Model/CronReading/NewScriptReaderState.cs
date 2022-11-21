using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Domain.Discovery.CronParsing.ReaderState;
using SteerMyWheel.Domain.Model.ReaderState;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.ReaderStates
{
    public class NewScriptState : IReaderState
    {
        public ScriptExecution newScript { get; set; }
        public NewScriptState(ScriptExecution script)
        {
            newScript = script;
        }

        public async Task handle(ReaderStateContext context)
        {
            await context._writer.WriteScriptExecution(newScript, context.currentHostName);
        }
    }
}
