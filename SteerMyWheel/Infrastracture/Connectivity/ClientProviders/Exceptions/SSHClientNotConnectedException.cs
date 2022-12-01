using System;
using System.Runtime.Serialization;

namespace SteerMyWheel.Infrastracture.Connectivity.ClientProviders.Exceptions
{
    [Serializable]
    public class SSHClientNotConnectedException : Exception
    {
        private static string _message = "The SSH client is not yet connected to a remote host. Consider verifying the provided connection informations.";
        public SSHClientNotConnectedException() : base(_message)
        {

        }

        public SSHClientNotConnectedException(string message) : base(message)
        {

        }

        public SSHClientNotConnectedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SSHClientNotConnectedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}