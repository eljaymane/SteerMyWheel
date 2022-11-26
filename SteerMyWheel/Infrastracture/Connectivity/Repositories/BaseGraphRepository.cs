using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Infrastracture.Connectivity.ClientProviders;
using System;
using System.Collections.Generic;

namespace SteerMyWheel.Infrastracture.Connectivity.Repositories
{
    public abstract class BaseGraphRepository<T, ID> : IGraphRepository<T, ID> where T : BaseEntity<ID> where ID : class
    {
        public NeoClientProvider _client;

        public BaseGraphRepository()
        {

        }

        public BaseGraphRepository(NeoClientProvider client)
        {
            _client = client;
        }

        public abstract T Create(T entity);
        public abstract T Delete(T entity);
        public abstract T Get(ID X);
        public abstract T Get(object entity);
        public abstract IEnumerable<T> GetAll();
        public abstract T Update(T entity);
        public abstract IEnumerable<T> GetAll(object entity);
       // public abstract IEnumerable<T> GetValues(T entity);
        public abstract Tuple<BaseEntity<ID>, object> Link(BaseEntity<ID> active, object passive);
        public abstract BaseEntity<ID> CreateAndMatch(BaseEntity<ID> newScript, string id);

    }
}
