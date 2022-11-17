using Neo4jClient.Cypher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Domain.Connectivity.ClientProvider
{
    public abstract class BaseClientProvider<T> : IDisposable, IClientProvider<T> where T : class
    {
        public abstract Task Connect();
        public abstract void Dispose();
        public abstract T GetConnection();

    }

    public abstract class BaseClientProvider<T, H> : IDisposable, IClientProvider<T, H> where T : class where H : class
    {
        public abstract Task Connect(H host);
        public abstract void Dispose();
        public abstract T GetConnection();
        public abstract bool isConnected();

    }
}
