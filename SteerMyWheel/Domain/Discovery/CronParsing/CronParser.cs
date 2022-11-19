using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SteerMyWheel.Configuration;
using SteerMyWheel.Domain.Model.ReaderState;
using SteerMyWheel.Domain.Discovery.CronParsing.ReaderState;
using SteerMyWheel.Core.Model.ReaderStates;
using SteerMyWheel.Core.Model.Entities;
using System.Drawing;

namespace SteerMyWheel.Domain.Discovery.CronParsing
{
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
        public IState Parse(string line)
        {
            _logger.LogInformation("[{time}] CronParser => Parsing line : {line}", DateTime.UtcNow, line);
            if (ParserConfig.IsScript(line)) return new NewScriptState(new ScriptExecution(_context.currentRole, GetCron(line), GetName(line), GetPath(line), GetExecCommand(line), ParserConfig.IsEnabled(line)));
            if (ParserConfig.IsRole(line)) return new NewRoleState(GetRole(line));
            if(ParserConfig.shouldIgnore(line)) return new IgnoreState();
            return null;
        }

        public static string GetRole(string line)
        {
            return line.Replace('#', ' ').TrimStart();
        }

        public static string GetCron(string line)
        {
            line = line.Replace('#', ' ').TrimStart();
            string[] values = line.Split(' ');
            return values[0] + ' ' + values[1] + ' ' + values[2] + ' ' + values[3] + ' ' + values[4];
        }

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

        public static string GetExecCommand(string line)
        {
            return new string(line.Skip(GetCron(line).Length).ToArray()).TrimStart();
        }
    }
}
