using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Neo4jClient;
using SteerMyWheel.CronParsing.Model;
using SteerMyWheel.Discovery.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
        private RemoteHost _host;

        public ScriptRepositoryService(GlobalConfig globalConfig,ILogger<ScriptRepositoryService> logger)
        {
            _configuration = globalConfig;
            _logger = logger;
        }

        public Task syncRepos()
        {
            var syncScripts = new TransformBlock<bool, IEnumerable<ScriptExecution>>(new Func<bool, IEnumerable<ScriptExecution>>(getAllScripts));
            var generateRepositories = new TransformBlock<IEnumerable<ScriptExecution>, Dictionary<ScriptRepository, List<ScriptExecution>>>(linkToRepository);
            var reflectChanges = new ActionBlock<Dictionary<ScriptRepository, List<ScriptExecution>>>(async _data =>
            {
                await reflectToGraph(_data);
            });
            syncScripts.LinkTo(generateRepositories);
            generateRepositories.LinkTo(reflectChanges);
            syncScripts.Completion.ContinueWith(delegate { generateRepositories.Complete(); });
            generateRepositories.Completion.ContinueWith(delegate { reflectChanges.Complete(); });
            syncScripts.Post(true);
            syncScripts.Complete();
            generateRepositories.Completion.Wait();
            reflectChanges.Completion.Wait();
            return Task.CompletedTask;
        }

        private IEnumerable<ScriptExecution> getAllScripts(bool firstExecution)
        {
            using (_client = new GraphClient(_configuration.neo4jRootURI, _configuration.neo4jUsername, _configuration.neo4jPassword))
            {
                _client.ConnectAsync().Wait();
                _client.DefaultDatabase = _configuration.neo4jDefaultDB;
                var scripts = _client.Cypher.Match("(script:ScriptExecution)")
                    .Return<ScriptExecution>(script => script.As<ScriptExecution>()).ResultsAsync.Result;
                return scripts;
            }
            
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
            using (_client = new GraphClient(_configuration.neo4jRootURI, _configuration.neo4jUsername, _configuration.neo4jPassword))
            {
                _client.ConnectAsync().Wait();
                _client.DefaultDatabase = _configuration.neo4jDefaultDB;
                try
                {
                    _client.Cypher.CreateUniqueConstraint("r:ScriptRepository", "r.name").ExecuteWithoutResultsAsync().Wait();
                }
                catch(Exception e)
                {

                }
                
                foreach (var repository in _repository.Keys)
                {
                    try
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
                    } catch(Exception e)
                    {

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
                        } catch(Exception e)
                        {

                        }
                        
                    }
                }
                return Task.CompletedTask;
            }

        }
    }
}
