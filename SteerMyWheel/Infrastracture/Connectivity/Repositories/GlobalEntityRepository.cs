using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Infrastracture.Connectivity.Repositories
{
    public class GlobalEntityRepository
    {
        public RemoteHostRepository RemoteHostRepository { get; }
        public ScriptExecutionRepository ScriptExecutionRepository { get; }
        public ScriptRepositoryRepository ScriptRepositoryRepository { get; }

        public GlobalEntityRepository(RemoteHostRepository remoteHostRepository, ScriptExecutionRepository scriptExecutionRepository, ScriptRepositoryRepository scriptRepositoryRepository)
        {
            RemoteHostRepository = remoteHostRepository;
            ScriptExecutionRepository = scriptExecutionRepository;
            ScriptRepositoryRepository = scriptRepositoryRepository;
        }

    }
}
