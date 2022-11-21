using SteerMyWheel.Domain.Model.ReaderState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.CronReading
{
    public class IgnoreReaderState : IReaderState
    {
        public Task handle(ReaderStateContext context)
        {
            return Task.CompletedTask;
        }
    }
}
