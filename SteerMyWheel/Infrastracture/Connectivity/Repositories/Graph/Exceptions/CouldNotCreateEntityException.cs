using SteerMyWheel.Core.Model.Entities;
using System;
using System.Runtime.Serialization;

namespace SteerMyWheel.Infrastracture.Connectivity.Repositories.Graph.Exceptions
{
    [Serializable]
    internal class CouldNotCreateEntityException<ID> : Exception
    {
        private static string _message = "Could not create entity";
        private BaseEntity<ID> _entity;

        public CouldNotCreateEntityException(BaseEntity<ID> e) : base(_message)
        {
            _entity = e;
        }

        public CouldNotCreateEntityException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CouldNotCreateEntityException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}