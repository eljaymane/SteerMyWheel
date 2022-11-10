using SteerMyWheel.CronParsing;
using SteerMyWheel.CronParsing.Model;
using SteerMyWheel.Writer;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteerMyWheel.Reader.ReaderStates
{
    public class NewScriptState : IState
    {
        public ScriptExecution newScript { get; set; }
        public NewScriptState(ScriptExecution script)
        {
            this.newScript = script;
        }

        public async void handle(ReaderStateContext context)
        {
            await context.Writer.WriteAsync(newScript);
        }
    }
}
