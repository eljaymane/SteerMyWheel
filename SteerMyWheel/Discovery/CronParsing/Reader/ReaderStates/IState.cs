using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Reader.ReaderStates
{
    public interface IState
    {
        Task handle(ReaderStateContext context);
    }
}
