using System;
using System.Runtime.Serialization;

namespace SteerMyWheel.Core.Model.CronReading.Exceptions
{
    [Serializable]
    public class ReaderStateContextNotInitializedException : Exception
    {
        private static string _message = "The reader context was not correctly initilized";
        public ReaderStateContextNotInitializedException()
        {
        }

        public ReaderStateContextNotInitializedException(string message) : base(message)
        {
        }

        public ReaderStateContextNotInitializedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ReaderStateContextNotInitializedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}