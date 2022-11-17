using SteerMyWheel.Core.Connectivity.ClientProviders;
using SteerMyWheel.Domain.Model.Entity;
using System.Collections.Generic;

namespace SteerMyWheel.Domain.Connectivity.GraphRepository
{
    public abstract class BaseGraphRepository<T, ID> : IGraphRepository<T, ID> where T : BaseEntity<ID> where ID : class
    {
        public NeoClientProvider _client;

        public BaseGraphRepository(NeoClientProvider client)
        {
            _client = client;
        }

        public abstract T Create(T entity);
        public abstract T Delete(T entity);
        public abstract T Get(ID X);
        public abstract IEnumerable<T> GetAll();
        public abstract T Update(T entity);

    }
}
