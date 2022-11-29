using SteerMyWheel.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Infrastracture.Connectivity.Git
{
    public class GitCommands
    {
        public static void PushAll(string path, bool sslVerify)
        {
            WinAPI.system($"cd {path} && git -c http.sslVerify={sslVerify} push --all");
        }

        public static void Push(string path, bool sslVerify)
        {
            WinAPI.system($"cd {path} && git -c http.sslVerify={sslVerify} push");
        }
        public static void RemoveRemote(string path, string remote)
        {
            WinAPI.system($"cd {path} && git remote rm {remote}");
        }
        public static void AddRemote(string path, string remoteName, string remote)
        {
            WinAPI.system($"cd {path} && git remote add {remoteName} {remote}");
        }

        public static void AddAllFiles(string path)
        {
            WinAPI.system($"cd {path} && git add .");
        }

        public static void CreateCommit(string path, string message)
        {
            WinAPI.system($"cd {path} && git commit -m \"{message}\"");
        }

        public static void Clone(string path, string remote, bool sslVerify)
        {
            WinAPI.system($"cd {path} && git -c http.sslVerify={sslVerify} clone {remote}");
        }

        public static void Pull(string path, bool sslVerify)
        {
            WinAPI.system($"cd {path} && git -c http.sslVerify={sslVerify} pull");
        }


    }
}
