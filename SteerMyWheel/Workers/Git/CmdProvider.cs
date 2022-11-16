using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SteerMyWheel.Workers.Git
{
    public static class WinAPI
    {
        [DllImport("msvcrt.dll")]
        public static extern int system(string cmd);

        [DllImport("kernel32.dll")]
        public static extern int GetProcessId(IntPtr handle);
    }
}
