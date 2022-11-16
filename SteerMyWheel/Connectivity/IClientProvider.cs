using Neo4jClient.Cypher;
using System.Threading.Tasks;

namespace SteerMyWheel.Connectivity
{
    public interface IClientProvider<T>
    {
        T GetConnection();

        Task Connect();
    }

}