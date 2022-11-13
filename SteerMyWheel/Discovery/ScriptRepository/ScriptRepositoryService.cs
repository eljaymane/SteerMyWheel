using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Neo4jClient;
using SteerMyWheel.Connectivity;
using SteerMyWheel.CronParsing.Model;
using SteerMyWheel.Discovery.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SteerMyWheel.Discovery.ScriptToRepository
{
    public class ScriptRepositoryService
    {
        private readonly GlobalConfig _configuration;
        private readonly ILogger<ScriptRepositoryService> _logger;
        private GraphClient _client;

        public ScriptRepositoryService(NeoClient client,ILogger<ScriptRepositoryService> logger)
        {
            _client = client.GetConnection();
            _logger = logger;
        }

        public Task syncRepos(RemoteHost host)
        {
            var syncScripts = new TransformBlock<RemoteHost, IEnumerable<ScriptExecution>>(new Func<RemoteHost, IEnumerable<ScriptExecution>>(getAllScripts));
            var generateRepositories = new TransformBlock<IEnumerable<ScriptExecution>, Dictionary<ScriptRepository, List<ScriptExecution>>>(linkToRepository);
            var reflectChanges = new ActionBlock<Dictionary<ScriptRepository, List<ScriptExecution>>>(async _data =>
            {
                await reflectToGraph(_data);
            });
            syncScripts.LinkTo(generateRepositories);
            generateRepositories.LinkTo(reflectChanges);
            syncScripts.Completion.ContinueWith(delegate { generateRepositories.Complete(); });
            generateRepositories.Completion.ContinueWith(delegate { reflectChanges.Complete(); });
            syncScripts.Post(host);
            syncScripts.Complete();
            generateRepositories.Completion.Wait();
            reflectChanges.Completion.Wait();
            return Task.CompletedTask;
        }

        private IEnumerable<ScriptExecution> getAllScripts(RemoteHost host)
        {
                _logger.LogInformation("[{time}] Requesting all scripts for host {host} ...", DateTime.UtcNow, host.name);
                var scripts = _client.Cypher.Match("(script:ScriptExecution)")
                    .Return<ScriptExecution>(script => script.As<ScriptExecution>()).ResultsAsync.Result;
                _logger.LogInformation("[{time}] Retrieved {count} script execution on {host} ...", DateTime.UtcNow, scripts.Count(),host.name);
                return scripts;
            
            
        }

        private Dictionary<ScriptRepository, List<ScriptExecution>> linkToRepository(IEnumerable<ScriptExecution> scripts)
        {
            Dictionary<ScriptRepository, List<ScriptExecution>> scriptRepository = new Dictionary<ScriptRepository, List<ScriptExecution>>();
            foreach (var script in scripts)
            {
                var repositoryName = RepositoryParser.getRepositoryName(script);
                var repository = new ScriptRepository(script.path,repositoryName);

                if (scriptRepository.ContainsKey(repository)) scriptRepository.GetValueOrDefault(repository).Add(script);
                else
                {
                    var list = new List<ScriptExecution>();
                    list.Add(script);
                    scriptRepository.Add(repository, list);
                }
            }
            return scriptRepository;
        }

        private Task reflectToGraph(Dictionary<ScriptRepository,List<ScriptExecution>> _repository)
        {
           
                try
                {
                    _client.Cypher.CreateUniqueConstraint("r:ScriptRepository", "r.name").ExecuteWithoutResultsAsync().Wait();
                    _logger.LogInformation("[{time}] Created unique constraint on ScriptRepository.name  ...", DateTime.UtcNow);
                }
                catch(Exception e)
                {
                    _logger.LogInformation("[{time}] Unique constraint on ScriptRepository.name already exists !", DateTime.UtcNow);
                }
                
                foreach (var repository in _repository.Keys)
                {
                    try
                    {
                    if(repository.name.Trim() == "" || repository.name.Contains("crontab")) _logger.LogInformation("[{time}] {name} Doesn't look like a repository !", DateTime.UtcNow, repository.name);
                    else
                            {
                                _client.Cypher.Merge("(scriptRepository:ScriptRepository { name : $name })")
                           .OnCreate()
                           .Set("scriptRepository = $repository")
                           .WithParams(new
                           {
                               name = repository.name,
                               repository = repository
                           })
                           .ExecuteWithoutResultsAsync().Wait();
                                _logger.LogInformation("[{time}] Created new ScriptRepository {name}  ...", DateTime.UtcNow, repository.name);
                            }
                       
                    } catch(Exception e)
                    {
                        _logger.LogInformation("[{time}] ScriptRepository {name} already exists !", DateTime.UtcNow, repository.name);
                    }
                    
                  
                    foreach (var script in _repository.GetValueOrDefault(repository))
                    {
                        try
                        {
                            _client.Cypher.Match("(r:ScriptRepository)","(s:ScriptExecution)")
                            .Where((ScriptRepository r) => r.name == repository.name)
                            .AndWhere((ScriptExecution s) => s.execCommand == script.execCommand)
                            .Create("(s)-[:IS_ON]->(r)")
                            .ExecuteWithoutResultsAsync().Wait();
                            _logger.LogInformation("[{time}] Linking ScriptExecution {ScriptName} to ScriptRepository {name}  ...", DateTime.UtcNow, script.name, repository.name);
                        } catch(Exception e)
                        {
                            _logger.LogInformation("[{time}] Could not link ScriptExecution {ScriptName} to ScriptRepository {name} ...", DateTime.UtcNow, script.name, repository.name);
                        }
                        
                    }
                }
                return Task.CompletedTask;
            

        }
    }
}
