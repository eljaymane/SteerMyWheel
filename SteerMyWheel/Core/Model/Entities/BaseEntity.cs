using System;

namespace SteerMyWheel.Core.Model.Entities
{
    public abstract class BaseEntity<ID> : IEquatable<BaseEntity<ID>>
    {
        public abstract bool Equals(BaseEntity<ID> other);

        public abstract ID GetID();

    }
}