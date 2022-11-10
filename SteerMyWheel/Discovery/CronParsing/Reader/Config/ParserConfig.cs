using System;
using System.Collections.Generic;
using System.Text;

namespace SteerMyWheel.Reader.Config
{
    public static class ParserConfig
    {
        // CRONEXPR + /home/kch-front/scripts/IDBPostTradeReport/bin/postTradeReport.pl
        public static string NameSimpleCase = "(.+\\/)";
        public static string NameJavaCase = @"(\w|\.|-)*(\.jar)";
        public static string Path = "(\\/)((\\w|-|_|[0-9])*\\/(\\w|-|_|[0-9])*)+[^.*\\.\\w]";
        public static string RepositoryNameJava = "(?<=\\/scripts\\/)(.*)";
        public static bool isJava(string line) { return line.Contains("java"); }
        public static bool isStdoRedirect(string line) { return line.Contains(">"); }
        public static bool IsRole(String line)
        {
            if (line.StartsWith('#') && !line.Contains('*')) return true;
            return false;
        }

        public static bool IsScript(String line)
        {
            return !IsRole(line);
        }

        public static bool IsEnabled(String line)
        {
            return !line.Trim().StartsWith('#');
        }
    }
}
