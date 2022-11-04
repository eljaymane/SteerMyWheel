using System;
using System.Collections.Generic;
using System.Text;

namespace SteerMyWheel.Reader.ReaderStates
{
    public interface IState
    {
        void handle(ReaderStateContext context);
    }
}
