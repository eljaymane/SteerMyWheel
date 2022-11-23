using Neo4jClient.Cypher;
using Renci.SshNet;
using System.Threading.Tasks;

namespace SteerMyWheel.Domain.Connectivity.ClientProvider
{
    public interface IClientProvider<T, H> where T : class where H : class
    {
        T GetConnection();
        Task ConnectSSH(H host);

        bool isConnected();
    }
    public interface IClientProvider<T> where T : class
    {
        T GetConnection();

        Task Connect();
    }



}