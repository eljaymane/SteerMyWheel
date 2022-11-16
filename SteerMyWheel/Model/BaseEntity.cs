using SteerMyWheel.Writer;
using System;

namespace SteerMyWheel.Model
{
    public abstract class BaseEntity<ID> : IWritable, IEquatable<BaseEntity<ID>>
    {
        public abstract bool Equals(BaseEntity<ID> other);

    }
}