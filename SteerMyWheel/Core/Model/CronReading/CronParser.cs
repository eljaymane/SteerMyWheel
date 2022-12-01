using Microsoft.Extensions.Logging;
using SteerMyWheel.Configuration;
using SteerMyWheel.Core.Model.Entities;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SteerMyWheel.Core.Model.CronReading
{
    /// <summary>
    /// The parser used to process a cron file lines to retrieve all the executions and their role.
    /// The parser expects the cron file to contain a commented line that explicites the role, followed by the cron lines...
    /// </summary>
    public class CronParser
    {
        private static ILogger<CronParser> _logger;
        private static ReaderStateContext _context;

        public CronParser()
        {

        }
        public CronParser(ILogger<CronParser> logger)
        {
            _logger = logger;
        }
        public void setContext(ReaderStateContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Parses a cron line to determine wether it is a script line, a role line or a line to ignore.
        /// </summary>
        /// <param name="line">The line to be parsed</param>
        /// <returns>The correct state that corresponds to the given line</returns>
        public IReaderState Parse(string line)
        {
            _logger.LogInformation("[{time}] CronParser => Parsing line : {line}", DateTime.UtcNow, line);
            if (ParserConfig.IsScript(line)) return new NewScriptReaderState(new ScriptExecution(_context.currentRole, GetCron(line), GetName(line), GetPath(line), GetExecCommand(line), ParserConfig.IsEnabled(line)));
            if (ParserConfig.IsRole(line)) return new NewRoleReaderState(GetRole(line));
            if (ParserConfig.shouldIgnore(line)) return new IgnoreReaderState();
            return null;
        }
        /// <summary>
        /// Extracts the role from a given line.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static string GetRole(string line)
        {
            return line.Replace('#', ' ').TrimStart();
        }
        /// <summary>
        /// Extracts the cron expression from a line
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static string GetCron(string line)
        {
            line = line.Replace('#', ' ').TrimStart();
            string[] values = line.Split(' ');
            return values[0] + ' ' + values[1] + ' ' + values[2] + ' ' + values[3] + ' ' + values[4];
        }
        /// <summary>
        /// Gets the name of the referenced script from the given line.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static string GetName(string line)
        {
            line = line.Replace('#', ' ').TrimStart();
            string name = string.Empty;
            string _line = GetExecCommand(line);
            if (_line.Contains('>'))
            {
                name = _line.Split('>')[0];
                name = name.Contains('>') ? name.Replace('>', ' ') : name;
                var _name = name.Split('/');
                name = _name[_name.Length - 1];
            }
            else if (_line.Contains("java"))
            {
                name = Regex.Match(_line, ParserConfig.NameJavaCase).ToString();
            }
            else
            {
                name = Regex.Replace(_line, ParserConfig.NameSimpleCase, "");
            }
            return name.Trim();
        }
        /// <summary>
        /// Extracts the path of the script execution from the given line.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static string GetPath(string line)
        {
            line = line.Replace('#', ' ').TrimStart();
            string path = GetExecCommand(line) == line ? line : string.Empty;
            if (line.Contains("&&") && !ParserConfig.isJava(line))
            {
                path = Regex.Matches(GetExecCommand(line), ParserConfig.Path).ElementAt(0).ToString();
            }
            else if (ParserConfig.isStdoRedirect(line))
            {
                path = line.Split('>')[0];
                return GetPath(path);
            }
            else
            {
                var matches = Regex.Matches(GetExecCommand(line), ParserConfig.Path);
                if (matches.Count > 1) path = matches.ElementAt(1).Value;
                else if (matches.Count == 0) path = "";
                else path = matches.ElementAt(0).Value;
            }
            return path.Trim();
        }
        /// <summary>
        /// Extracts the execution command from the given line.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static string GetExecCommand(string line)
        {
            return new string(line.Skip(GetCron(line).Length).ToArray()).TrimStart();
        }
    }
}
