using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SteerMyWheel.Workers.Git
{
    public static class CmdProvider
    {
        [DllImport("msvcrt.dll")]
        public static extern int system(string cmd);
    }
}
