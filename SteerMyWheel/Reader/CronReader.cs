using SteerMyWheel.Reader.ReaderStates;
using SteerMyWheel.Model;
using SteerMyWheel.Writer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SteerMyWheel.Reader
{
    public class CronReader
    {
        private ReaderStateContext _stateContext;
        private readonly String[] _cronFile;
        public CronReader(String cronFilePath, Host host,ILoggerFactory loggerFactory)
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
