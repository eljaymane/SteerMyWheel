using SteerMyWheel.Core.Model.Entities;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SteerMyWheel.Configuration
{
    public static class ParserConfig
    {
        // CRONEXPR + /home/kch-front/scripts/IDBPostTradeReport/bin/postTradeReport.pl
        public static string NameSimpleCase = "(.+\\/)";
        public static string NameJavaCase = @"(\w|\.|-)*(\.jar)";
        public static string Path = "(\\/)((\\w|-|_|[0-9])*\\/(\\w|-|_|[0-9])*)+[^.*\\.\\w]";
        public static string RepositoryName = "(?<=\\/scripts\\/)(.*)";
        public static bool isPython(string name) { return name.Split(".")?[1] == "py"; }
        public static bool isPerl(string name) { return (!name.Contains(".") || name.Split(".")?[1] == "pl") ? true : false; }
        public static bool isBash(string name) { return name.Split('.')?[1] == "sh" ? true : false; }
        public static bool isJava(string line) { return line.Contains("java"); }
        public static bool isStdoRedirect(string line) { return line.Contains(">"); }

        public static List<string> IgnoreDirectories
        {
            get
            {
                return new List<string> {
            "Archivage","data","log","logs"
            };
            }
        }
        public static bool shouldIgnore(string line)
        {
            return line.Replace("#", "").Replace("-", "").Trim() == "";
        }
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
