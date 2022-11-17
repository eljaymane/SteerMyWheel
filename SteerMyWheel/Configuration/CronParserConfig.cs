using SteerMyWheel.Core.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SteerMyWheel.Configuration
{
    public static class CronParserConfig
    {
        // CRONEXPR + /home/kch-front/scripts/IDBPostTradeReport/bin/postTradeReport.pl
        public static string NameSimpleCase = "(.+\\/)";
        public static string NameJavaCase = @"(\w|\.|-)*(\.jar)";
        public static string Path = "(\\/)((\\w|-|_|[0-9])*\\/(\\w|-|_|[0-9])*)+[^.*\\.\\w]";
        public static string RepositoryName = "(?<=\\/scripts\\/)(.*)";
        public static bool isJava(string line) { return line.Contains("java"); }
        public static bool isStdoRedirect(string line) { return line.Contains(">"); }
        public static bool IsRole(string line)
        {
            if (line.StartsWith('#') && !line.Contains('*')) return true;
            return false;
        }

        public static bool IsScript(string line)
        {
            return !IsRole(line);
        }

        public static bool IsEnabled(string line)
        {
            return !line.Trim().StartsWith('#');
        }

        public static string getRepositoryName(ScriptExecution _script)
        {
            var name = Regex.Match(_script.Path, RepositoryName).ToString();
            name = name.Contains('/') ? name.Split('/')[0] : name;
            return name;

        }
    }
}
