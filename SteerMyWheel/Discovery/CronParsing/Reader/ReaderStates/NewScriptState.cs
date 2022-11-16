using SteerMyWheel.Model;
using System.Threading.Tasks;

namespace SteerMyWheel.Reader.ReaderStates
{
    public class NewScriptState : IState
    {
        public ScriptExecution newScript { get; set; }
        public NewScriptState(ScriptExecution script)
        {
            this.newScript = script;
        }

        public async Task handle(ReaderStateContext context)
        {
            await context._writer.WriteAsync(newScript);
        }
    }
}
