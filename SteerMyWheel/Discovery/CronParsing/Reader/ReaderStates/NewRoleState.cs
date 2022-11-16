using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Reader.ReaderStates
{
    public class NewRoleState : IState
    {
        private String scriptRole { get; }
        public NewRoleState(String role)
        {
            this.scriptRole = role;
        }
        public async Task handle(ReaderStateContext context)
        {
            context.currentRole = this.scriptRole;
        }
    }
}
