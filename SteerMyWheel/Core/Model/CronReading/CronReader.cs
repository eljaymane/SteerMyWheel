using System.IO;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.CronReading
{
    /// <summary>
    /// The main role of this class is to read a cron file either from memory as text or from a local file.
    /// It then passes every line of the cron file to CronParser for the parsing task.
    /// </summary>
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
        /// <summary>
        /// Reads all the lines of a given cron file and the parse every line.
        /// </summary>
        /// <param name="cronFilePath"></param>
        /// <returns></returns>
        public Task ReadFromFile(string cronFilePath)
        {
            var _cronFile = File.ReadAllLines(cronFilePath);
            foreach (var _line in _cronFile)
            {
                if (_line != "") Parse(_line);
            }
            return Task.CompletedTask;
        }
        /// <summary>
        /// Reads a cron file stored in memory and splits it into line for the parsing process.
        /// </summary>
        /// <param name="cronText"></param>
        /// <returns></returns>
        public Task ReadFromText(string cronText)
        {
            var _cronFile = cronText.Split('\n');
            foreach (var _line in _cronFile)
            {
                if (_line != "") Parse(_line);
            }
            return Task.CompletedTask;
        }
        /// <summary>
        /// Parses a given line and calls setState to update the actual reading context state.
        /// </summary>
        /// <param name="line"></param>
        public void Parse(string line)
        {
            _stateContext.setState(_parser.Parse(line));
        }

    }
}
