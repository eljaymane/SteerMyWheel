using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Neo4jClient;
using Newtonsoft.Json.Linq;
using SteerMyWheel.Connectivity;
using SteerMyWheel.CronParsing.Model;
using SteerMyWheel.Discovery.Model;
using SteerMyWheel.TaskQueue;
using SteerMyWheel.Workers.Git;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SteerMyWheel.Discovery.ScriptToRepository
{
    public class ScriptRepositoryService
    {
        private ILoggerFactory _loggerFactory;
        private readonly ILogger<ScriptRepositoryService> _logger;
        private GraphClient _client;
        private NeoClient _neoClient;
        private BitbucketClient _bitClient;
        private WorkQueue<GitMigrationWorker> _gitQueue;

        public ScriptRepositoryService(BitbucketClient bitClient,NeoClient client,ILogger<ScriptRepositoryService> logger,WorkQueue<GitMigrationWorker> queue)
        {
            _client = client.GetConnection();
            _neoClient = client;
            _logger = logger;
            _gitQueue = queue;
            _bitClient = bitClient;
        }

        public void setLoggerFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public Task generateGraphRepos(RemoteHost host)
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

        public Task syncRepos(RemoteHost host)
        {
            var getRepos = new TransformBlock<RemoteHost, IEnumerable<ScriptRepository>>(new Func<RemoteHost, IEnumerable<ScriptRepository>>(getAllRepositories));
            var sync = new TransformBlock<IEnumerable<ScriptRepository>,IEnumerable<ScriptRepository>>(async data => {
                return generateMigrationWorkers(data);
            });
            var genReport = new ActionBlock<IEnumerable<ScriptRepository>>(data =>
            {
                generateReport(data, host).Wait();
            });
            getRepos.LinkTo(sync);
            sync.LinkTo(genReport);
            getRepos.Completion.ContinueWith(delegate { sync.Complete(); });
            sync.Completion.ContinueWith(delegate { genReport.Complete(); });
            getRepos.Post(host);
            getRepos.Complete();
            sync.Completion.Wait();
            genReport.Completion.Wait();
            
            //sync.Completion.Wait();
            return Task.CompletedTask;
        }

        private Task generateReport(IEnumerable<ScriptRepository> repositories,RemoteHost host)
        {
            var txt = "RemoteHost;Repository\n";
            foreach (var repo in repositories)
            {
                txt += $"{host.name};{repo.name}\n";
            }
            File.WriteAllText($"C:/steer/report_{host.name}.txt", txt);
            return Task.CompletedTask;
        }


        private IEnumerable<ScriptRepository> getAllRepositories(RemoteHost host)
        {
            _logger.LogInformation("[{time}] Requesting all repositories for host {host} ...", DateTime.UtcNow, host.name);
            var result = _client.Cypher
                .Match("(h:RemoteHost)-[:HOSTS]-(se:ScriptExecution)-[:IS_ON]->(s:ScriptRepository)")
                .Where((RemoteHost h) => h.name == host.name)
                .ReturnDistinct(s => s.As<ScriptRepository>())
                .ResultsAsync.Result;
            _logger.LogInformation("[{time}] Retrieved {count} ScriptRepositories ...", DateTime.UtcNow, result.Count());
            return result;

        }

        private IEnumerable<ScriptRepository> generateMigrationWorkers(IEnumerable<ScriptRepository> repositories)
        {
            foreach (var repo in repositories)
            {
                var worker = new GitMigrationWorker(repo);
                worker.setLogger(_loggerFactory.CreateLogger<GitMigrationWorker>());
                worker.setClient(this._neoClient);
                worker.setBitBucketClient(this._bitClient);
                _gitQueue.Enqueue(worker).Wait();
            }
            var token = new CancellationTokenSource(TimeSpan.FromSeconds(90)).Token;
             _gitQueue.DeqeueAllAsync(token).Wait();
            var verify = Task.Run(() =>
            {
                var list = Directory.GetDirectories("C:/steer/");
                using (var client = _client)
                {
                    foreach (var dir in list)
                    {
                        var name = dir.Replace("C:/steer/", "");
                        _logger.LogInformation("[{time}] Repository {name} successfully cloned ..", DateTime.UtcNow, name);
                        client.Cypher.Match("(s:ScriptRepository)")
                       .Where((ScriptRepository s) => s.name == name)
                       .Set("s.isCloned = true")
                       .ExecuteWithoutResultsAsync().Wait();
                    }
                }

            });
            verify.Wait();
            return repositories;


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
