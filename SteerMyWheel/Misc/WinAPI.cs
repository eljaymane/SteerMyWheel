using System;
using System.Runtime.InteropServices;

namespace SteerMyWheel.Misc
{
    public static class WinAPI
    {
        [DllImport("msvcrt.dll")]
        public static extern int system(string cmd);

        [DllImport("kernel32.dll")]
        public static extern int GetProcessId(IntPtr handle);
    }
}
