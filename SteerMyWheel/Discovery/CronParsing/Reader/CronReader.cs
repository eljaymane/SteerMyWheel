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
        private CronParser _parser;
        public CronReader(ReaderStateContext stateContext,CronParser parser)
        {
            _stateContext = stateContext;
            _parser = parser;
            _parser.setContext(stateContext);
        }

        public ReaderStateContext GetContext()
        {
            return _stateContext;
        }
        public void Read(String cronFilePath)
        {
            var _cronFile = File.ReadAllLines(cronFilePath);
            foreach (var _line in _cronFile)
            {
                if(_line != "") this.Parse(_line);
            }
        }

        public void Parse(String line)
        {
            _stateContext.setState(_parser.Parse(line));
        }
        public async void Write()
        {
            if (_stateContext.currentState.GetType() != typeof(NewScriptState))
                return;
            await this._stateContext._writer.WriteAsync(((NewScriptState)_stateContext.currentState).newScript);
        }
    }
}
