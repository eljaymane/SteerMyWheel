using SteerMyWheel.Reader.Config;
using SteerMyWheel.Reader.ReaderStates;
using SteerMyWheel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace SteerMyWheel.Reader
{
     public class CronParser
    {
        public static ReaderStateContext _context;
        public static IState Parse(String line)
        {
            if (ParserConfig.IsScript(line)) return new NewScriptState(new Script(_context.currentRole,GetCron(line), GetName(line), GetPath(line),GetExecCommand(line), ParserConfig.IsEnabled(line)));
            if (ParserConfig.IsRole(line)) return new NewRoleState(GetRole(line));
            return null;
        }

        public static String GetRole(String line)
        {
            return line.Replace('#', ' ').TrimStart();
        }

        public static String GetCron(String line)
        {
            line = line.Replace('#', ' ').TrimStart();
            String[] values = line.Split(' ');
            return values[0] + ' ' + values[1] + ' ' + values[2] + ' ' + values[3] + ' ' + values[4];
        }

        public static String GetName(String line)
        {
            line = line.Replace('#', ' ').TrimStart();
            String name = String.Empty;
            String _line = GetExecCommand(line);
            if (_line.Contains('>'))
            {
                name = _line.Split('>')[0];
                name = name.Contains('>') ? name.Replace('>', ' ') : name;
                var _name = name.Split('/');
                name = _name[_name.Length - 1];
            } else if (_line.Contains("java"))
            {
                name = Regex.Match(_line, ParserConfig.NameJavaCase).ToString();
            } else
            {
                name = Regex.Replace(_line, ParserConfig.NameSimpleCase, "");
            }
            return name.Trim();
        }

        public static String GetPath(String line)
        {
            line = line.Replace('#', ' ').TrimStart();
            String path = GetExecCommand(line)==line?line: String.Empty;
            if(line.Contains("&&") && !ParserConfig.isJava(line))
            {
                path = Regex.Matches(GetExecCommand(line), ParserConfig.Path).ElementAt(0).ToString();
            } else if (ParserConfig.isStdoRedirect(line))
            {
                path = line.Split('>')[0];
                return GetPath(path);
            } else
            {
                var matches = Regex.Matches(GetExecCommand(line), ParserConfig.Path);
                if (matches.Count > 1) path = matches.ElementAt(1).Value;
                else if (matches.Count == 0) path = "";
                else path = matches.ElementAt(0).Value;
            }
            return path.Trim();
        }
        
        public static string GetExecCommand(String line)
        {
            return new String(line.Skip(GetCron(line).Length).ToArray()).TrimStart();
        }
    }
}
