using System;
using System.Collections.Generic;
using System.Text;

namespace SteerMyWheel.Reader.ReaderStates
{
    public class NewRoleState : IState
    {
        private String scriptRole { get; }
        public NewRoleState(String role)
        {
            this.scriptRole = role;
        }
        public void handle(ReaderStateContext context)
        {
            context.currentRole = this.scriptRole;
        }
    }
}
