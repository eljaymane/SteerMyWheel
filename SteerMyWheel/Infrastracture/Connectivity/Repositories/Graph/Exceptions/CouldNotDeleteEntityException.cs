using System;
using System.Runtime.Serialization;

namespace SteerMyWheel.Infrastracture.Connectivity.Repositories.Graph.Exceptions
{
    [Serializable]
    internal class CouldNotDeleteEntityException<ID> : Exception where ID : class
    {
        private object entity;

        public CouldNotDeleteEntityException()
        {
        }

        public CouldNotDeleteEntityException(object entity)
        {
            this.entity = entity;
        }

        public CouldNotDeleteEntityException(string message) : base(message)
        {
        }

        public CouldNotDeleteEntityException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CouldNotDeleteEntityException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}