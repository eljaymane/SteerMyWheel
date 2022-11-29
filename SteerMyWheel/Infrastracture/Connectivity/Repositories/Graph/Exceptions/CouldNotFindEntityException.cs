using Microsoft.Extensions.Logging;
using SteerMyWheel.Core.Model.Entities;
using System;
using System.Runtime.Serialization;

namespace SteerMyWheel.Infrastracture.Connectivity.Repositories.Graph.Exceptions
{
    [Serializable]
    internal class CouldNotFindEntityException<ID> : Exception
    {

        public CouldNotFindEntityException(ILogger<BaseEntity<ID>> _logger, BaseEntity<ID> e)
        {
            _logger.LogError($"Could not find entity : {e.GetID()} ");
        }

        public CouldNotFindEntityException(string message) : base(message)
        {
        }

        public CouldNotFindEntityException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CouldNotFindEntityException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}