using SteerMyWheel.Domain.Discovery.CronParsing.ReaderState;
using SteerMyWheel.Domain.Model.ReaderState;
using System;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.ReaderStates
{
    public class NewRoleState : IState
    {
        private string scriptRole { get; }
        public NewRoleState(string role)
        {
            scriptRole = role;
        }
        public async Task handle(ReaderStateContext context)
        {
            context.currentRole = scriptRole;
        }
    }
}
