using SteerMyWheel.Reader.ReaderStates;
using System;
using System.IO;
using Microsoft.Extensions.Logging;
using SteerMyWheel.CronParsing.Model;

namespace SteerMyWheel.Reader
{
    public class CronReader
    {
        private ReaderStateContext _stateContext;
        private readonly String[] _cronFile;
        public CronReader(String cronFilePath, RemoteHost host,ILoggerFactory loggerFactory)
        {
            _cronFile = File.ReadAllLines(cronFilePath);
            _stateContext = new ReaderStateContext(host,loggerFactory);
            CronParser._context = _stateContext;
        }

        public void Read()
        {
            foreach (var _line in _cronFile)
            {
                if(_line != "") this.Parse(_line);
            }
        }

        public void Parse(String line)
        {
            _stateContext.setState(CronParser.Parse(line));
        }
        public void Write()
        {
            if (_stateContext.currentState.GetType() != typeof(NewScriptState))
                return;
            this._stateContext.Writer.WriteAsync(((NewScriptState)_stateContext.currentState).newScript);
        }
    }
}
