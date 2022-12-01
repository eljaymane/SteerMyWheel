using SteerMyWheel.Core.Model.Entities;
using System;
using System.Collections.Generic;

namespace SteerMyWheel.Infrastracture.Connectivity.Repositories
{
    public interface IGraphRepository<T, ID> where T : BaseEntity<ID> where ID : class
    {
        T Create(T entity);
        T Get(ID X);
        IEnumerable<T> GetAll();
        IEnumerable<T> GetAll(object entity);
        T Update(T entity);
        T Delete(T entity);
        Tuple<BaseEntity<ID>, object> Link(BaseEntity<ID> active, object passive);
    }
}
