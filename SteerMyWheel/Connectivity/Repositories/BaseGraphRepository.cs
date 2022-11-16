using SteerMyWheel.Connectivity.ClientProviders;
using SteerMyWheel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Connectivity.Repositories
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


        public abstract T Update(T entity);
     
    }
}
