using SteerMyWheel.CronParsing.Model;
using SteerMyWheel.Reader;
using SteerMyWheel.Reader.Config;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SteerMyWheel.Discovery.ScriptToRepository
{
    public static class RepositoryParser
    {
        public static string getRepositoryName(ScriptExecution _script)
        {
            var name = Regex.Match(_script.path, ParserConfig.RepositoryNameJava).ToString();
            name = name.Contains('/') ? name.Split('/')[0] : name;
            return name ;
          
        }
    }
}
