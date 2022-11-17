using SteerMyWheel.Domain.Model.Entity;

namespace SteerMyWheel.Domain.Connectivity.GraphRepository
{
    public interface IGraphRepository<T, ID> where T : BaseEntity<ID> where ID : class
    {
        T Create(T entity);
        T Get(ID X);
        T Update(T entity);
        T Delete(T entity);

    }
}
