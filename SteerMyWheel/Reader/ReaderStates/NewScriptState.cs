using SteerMyWheel.Model;
using SteerMyWheel.Writer;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteerMyWheel.Reader.ReaderStates
{
    public class NewScriptState : IState
    {
        public Script newScript { get; set; }
        public NewScriptState(Script script)
        {
            this.newScript = script;
        }

        public async void handle(ReaderStateContext context)
        {
            await context.Writer.WriteAsync(newScript);
        }
    }
}
