using SteerMyWheel.Domain.Model.ReaderState;
using System;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.CronReading
{
    public class NewRoleReaderState : IReaderState
    {
        private string scriptRole { get; }
        public NewRoleReaderState(string role)
        {
            scriptRole = role;
        }
        public async Task handle(ReaderStateContext context)
        {
            context.currentRole = scriptRole;
        }
    }
}
