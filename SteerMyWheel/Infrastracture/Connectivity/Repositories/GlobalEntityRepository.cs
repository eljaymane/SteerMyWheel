using SteerMyWheel.Core.Connectivity.Repositories;
using SteerMyWheel.Core.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Infrastracture.Connectivity.Repositories
{
    public class GlobalEntityRepository
    {
        public BaseGraphRepository<RemoteHost,string> RemoteHostRepository { get; }
        public BaseGraphRepository<ScriptExecution,string> ScriptExecutionRepository { get; }
        public BaseGraphRepository<ScriptRepository,string> ScriptRepositoryRepository { get; }

        public GlobalEntityRepository(BaseGraphRepository<RemoteHost, string> remoteHostRepository, BaseGraphRepository<Core.Model.Entities.ScriptExecution, string> scriptExecutionRepository, BaseGraphRepository<Core.Model.Entities.ScriptRepository, string> scriptRepositoryRepository)
        {
            RemoteHostRepository = remoteHostRepository;
            ScriptExecutionRepository = scriptExecutionRepository;
            ScriptRepositoryRepository = scriptRepositoryRepository;
        }

    }
}
