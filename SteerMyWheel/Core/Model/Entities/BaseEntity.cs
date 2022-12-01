using System;

namespace SteerMyWheel.Core.Model.Entities
{
    /// <summary>
    /// The abstract base of the entities consumed by the built-in logic.
    /// Every baseEntity is storable in a graph database and has it's corresponding repository.
    /// </summary>
    /// <typeparam name="ID"></typeparam>
    public abstract class BaseEntity<ID> : IEquatable<BaseEntity<ID>>
    {
        /// <summary>
        /// Redefines the equality between two BaseEntities of the same type.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>True if equal, false if not.</returns>
        public abstract bool Equals(BaseEntity<ID> other);
        /// <summary>
        /// Returns the id of a given entity.
        /// </summary>
        /// <returns>The id of the entity.</returns>
        public abstract ID GetID();

    }
}