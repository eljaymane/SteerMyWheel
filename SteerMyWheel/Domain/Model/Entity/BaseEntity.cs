using SteerMyWheel.Domain.Model.Writer;
using System;

namespace SteerMyWheel.Domain.Model.Entity
{
    public abstract class BaseEntity<ID> : IWritable, IEquatable<BaseEntity<ID>>
    {
        public abstract bool Equals(BaseEntity<ID> other);

    }
}