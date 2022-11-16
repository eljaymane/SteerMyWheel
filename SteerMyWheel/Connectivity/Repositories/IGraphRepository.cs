using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SteerMyWheel.Model;

namespace SteerMyWheel.Connectivity.Repositories
{
    public interface IGraphRepository<T,ID> where T : BaseEntity<ID> where ID : class
    {
        T Create(T entity);
        T Get(ID X);
        T Update(T entity);
        T Delete(T entity);

    }
}
