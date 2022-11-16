using SteerMyWheel.Model;
using SteerMyWheel.Reader.Config;
using System.Text.RegularExpressions;

namespace SteerMyWheel.Discovery.ScriptToRepository
{
    public static class RepositoryParser
    {
        public static string getRepositoryName(ScriptExecution _script)
        {
            var name = Regex.Match(_script.Path, ParserConfig.RepositoryNameJava).ToString();
            name = name.Contains('/') ? name.Split('/')[0] : name;
            return name ;
          
        }
    }
}
