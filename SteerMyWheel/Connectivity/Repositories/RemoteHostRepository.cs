using Microsoft.Extensions.Hosting;
using SteerMyWheel.Connectivity.ClientProviders;
using SteerMyWheel.Model;
using System;
using System.Linq;

namespace SteerMyWheel.Connectivity.Repositories
{
    public class RemoteHostRepository : BaseGraphRepository<RemoteHost, string>
    {
        public RemoteHostRepository(NeoClientProvider client) : base(client)
        {
        }

        public override RemoteHost Create(RemoteHost entity)
        {
            using (var client = base._client.GetConnection())
            {
                try
                {
                   var result = client.Cypher.Merge("(remoteHost:RemoteHost { RemoteIP : $ip })")
                        .OnCreate()
                        .Set("remoteHost = $entity")
                        .WithParams(new
                        {
                            ip = entity.RemoteIP,
                            entity = entity
                        })
                         .Return<RemoteHost>(h => h.As<RemoteHost>()).ResultsAsync.Result.First();
                    return result;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
            return default(RemoteHost);
        }

        public override RemoteHost Delete(RemoteHost entity)
        {
            using (var client = base._client.GetConnection())
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
            return default(RemoteHost);
        }

        public override RemoteHost Get(string X)
        {
            using (var client = base._client.GetConnection())
            {
                try
                {
                    var entity = client.Cypher.Match("(remoteHost:RemoteHost)")
                         .Where((RemoteHost s) => s.RemoteIP == X)
                         .Return<RemoteHost>(s => s.As<RemoteHost>()).ResultsAsync.Result.First();
                    return entity;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
            return default(RemoteHost);
        }

        public override RemoteHost Update(RemoteHost entity)
        {
            using (var client = base._client.GetConnection())
            {
                try
                {
                    var result = client.Cypher.Match("(remoteHost:RemoteHost)")
                         .Where((RemoteHost s) => s.RemoteIP == entity.RemoteIP)
                         .Return<RemoteHost>(s => s.As<RemoteHost>()).ResultsAsync.Result.First();
                    return result;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
            return default(RemoteHost);
        }
    }
}
