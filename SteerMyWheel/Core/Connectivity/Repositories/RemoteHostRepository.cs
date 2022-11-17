using Microsoft.Extensions.Logging;
using SteerMyWheel.Core.Connectivity.ClientProviders;
using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Domain.Connectivity.GraphRepository;
using System;
using System.Linq;

namespace SteerMyWheel.Core.Connectivity.Repositories
{
    public class RemoteHostRepository : BaseGraphRepository<RemoteHost, string>
    {
        private readonly ILogger<RemoteHostRepository> _logger;
        public RemoteHostRepository(NeoClientProvider client, ILogger<RemoteHostRepository> logger) : base(client)
        {
            _logger = logger;
        }

        public override RemoteHost Create(RemoteHost entity)
        {
            _logger.LogInformation("[{time}] RemoteHostRepository => Creating host : {host}", DateTime.UtcNow, entity.ToString());
            using (var client = _client.GetConnection())
            {
                try
                {
                    var result = client.Cypher.Merge("(remoteHost:RemoteHost { RemoteIP : $ip })")
                         .OnCreate()
                         .Set("remoteHost = $entity")
                         .WithParams(new
                         {
                             ip = entity.RemoteIP,
                             entity
                         })
                          .Return(h => h.As<RemoteHost>()).ResultsAsync.Result.First();
                    return result;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
            return default;
        }

        public override RemoteHost Delete(RemoteHost entity)
        {
            using (var client = _client.GetConnection())
            {
                try
                {
                    client.Cypher.Match("(remoteHost:RemoteHost)")
                         .Where((RemoteHost h) => h.RemoteIP == entity.RemoteIP)
                         .Delete("remoteHost");
                    return entity;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        public override RemoteHost Get(string X)
        {
            using (var client = _client.GetConnection())
            {
                try
                {
                    var entity = client.Cypher.Match("(remoteHost:RemoteHost)")
                         .Where((RemoteHost s) => s.RemoteIP == X)
                         .Return(s => s.As<RemoteHost>()).ResultsAsync.Result.First();
                    return entity;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        public override RemoteHost Update(RemoteHost entity)
        {
            using (var client = _client.GetConnection())
            {
                try
                {
                    var result = client.Cypher.Match("(remoteHost:RemoteHost)")
                         .Where((RemoteHost s) => s.RemoteIP == entity.RemoteIP)
                         .Return(s => s.As<RemoteHost>()).ResultsAsync.Result.First();
                    return result;
                }
                catch (Exception e)
                {
                    return null;
                }
            }

        }
    }
}
