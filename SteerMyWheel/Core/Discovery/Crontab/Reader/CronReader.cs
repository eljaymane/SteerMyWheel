using SteerMyWheel.Core.Model.CronReading;
using SteerMyWheel.Domain.Discovery.CronParsing;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Discovery.Crontab.Reader
{
    public class CronReader
    {
        private ReaderStateContext _stateContext;
        private CronParser _parser;
        public CronReader(ReaderStateContext stateContext, CronParser parser)
        {
            _stateContext = stateContext;
            _parser = parser;
            _parser.setContext(stateContext);
        }

        public ReaderStateContext GetContext()
        {
            return _stateContext;
        }
        public Task ReadFromFile(string cronFilePath)
        {
            var _cronFile = File.ReadAllLines(cronFilePath);
            foreach (var _line in _cronFile)
            {
                if (_line != "") Parse(_line);
            }
            return Task.CompletedTask;
        }

        public Task ReadFromText(string cronText)
        {
            var _cronFile = cronText.Split('\n');
            foreach (var _line in _cronFile)
            {
                if (_line != "") Parse(_line);
            }
            return Task.CompletedTask;
        }

        public void Parse(string line)
        {
            _stateContext.setState(_parser.Parse(line));
        }
        public async void Write()
        {
            if (_stateContext.currentState.GetType() != typeof(NewScriptReaderState))
                return;
            await _stateContext._writer.WriteScriptExecution(((NewScriptReaderState)_stateContext.currentState).newScript,_stateContext.currentHostName);
        }
    }
}
