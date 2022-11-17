using Microsoft.Extensions.Logging;
using Neo4jClient;
using SteerMyWheel.Configuration;
using SteerMyWheel.Core.Connectivity.ClientProviders;
using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Core.Model.WorkersQueue;
using SteerMyWheel.Core.Synchronization.Migration.Git;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SteerMyWheel.Core.Synchronization
{
    public class ScriptSyncService
    {
        private ILoggerFactory _loggerFactory;
        private readonly ILogger<ScriptSyncService> _logger;
        private GraphClient _client;
        private NeoClientProvider _neoClient;
        private BitbucketClientProvider _bitClient;
        private WorkersQueue<GitMigrationWorker> _gitQueue;

        public ScriptSyncService(BitbucketClientProvider bitClient, NeoClientProvider client, ILogger<ScriptSyncService> logger, WorkersQueue<GitMigrationWorker> queue)
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
            var sync = new TransformBlock<IEnumerable<ScriptRepository>, IEnumerable<ScriptRepository>>(async data =>
            {
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

        private Task generateReport(IEnumerable<ScriptRepository> repositories, RemoteHost host)
        {
            var txt = "RemoteHost;Repository\n";
            foreach (var repo in repositories)
            {
                txt += $"{host.Name};{repo.Name}\n";
            }
            File.WriteAllText($"C:/steer/report_{host.Name}.txt", txt);
            return Task.CompletedTask;
        }


        private IEnumerable<ScriptRepository> getAllRepositories(RemoteHost host)
        {
            _logger.LogInformation("[{time}] Requesting all repositories for host {host} ...", DateTime.UtcNow, host.Name);
            var result = _client.Cypher
                .Match("(h:RemoteHost)-[:HOSTS]-(se:ScriptExecution)-[:IS_ON]->(s:ScriptRepository)")
                .Where((RemoteHost h) => h.Name == host.Name)
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
                worker.setClient(_neoClient);
                worker.setBitBucketClient(_bitClient);
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
                       .Where((ScriptRepository s) => s.Name == name)
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
            _logger.LogInformation("[{time}] Requesting all scripts for host {host} ...", DateTime.UtcNow, host.Name);
            var scripts = _client.Cypher.Match("(script:ScriptExecution)")
                .Return(script => script.As<ScriptExecution>()).ResultsAsync.Result;
            _logger.LogInformation("[{time}] Retrieved {count} script execution on {host} ...", DateTime.UtcNow, scripts.Count(), host.Name);
            return scripts;


        }

        private Dictionary<ScriptRepository, List<ScriptExecution>> linkToRepository(IEnumerable<ScriptExecution> scripts)
        {
            Dictionary<ScriptRepository, List<ScriptExecution>> scriptRepository = new Dictionary<ScriptRepository, List<ScriptExecution>>();
            foreach (var script in scripts)
            {
                var repositoryName = CronParserConfig.getRepositoryName(script);
                var repository = new ScriptRepository(script.Path, repositoryName);

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

        private Task reflectToGraph(Dictionary<ScriptRepository, List<ScriptExecution>> _repository)
        {

            try
            {
                _client.Cypher.CreateUniqueConstraint("r:ScriptRepository", "r.name").ExecuteWithoutResultsAsync().Wait();
                _logger.LogInformation("[{time}] Created unique constraint on ScriptRepository.name  ...", DateTime.UtcNow);
            }
            catch (Exception e)
            {
                _logger.LogInformation("[{time}] Unique constraint on ScriptRepository.name already exists !", DateTime.UtcNow);
            }

            foreach (var repository in _repository.Keys)
            {
                try
                {
                    if (repository.Name.Trim() == "" || repository.Name.Contains("crontab")) _logger.LogInformation("[{time}] {name} Doesn't look like a repository !", DateTime.UtcNow, repository.Name);
                    else
                    {
                        _client.Cypher.Merge("(scriptRepository:ScriptRepository { name : $name })")
                   .OnCreate()
                   .Set("scriptRepository = $repository")
                   .WithParams(new
                   {
                       name = repository.Name,
                       repository
                   })
                   .ExecuteWithoutResultsAsync().Wait();
                        _logger.LogInformation("[{time}] Created new ScriptRepository {name}  ...", DateTime.UtcNow, repository.Name);
                    }

                }
                catch (Exception e)
                {
                    _logger.LogInformation("[{time}] ScriptRepository {name} already exists !", DateTime.UtcNow, repository.Name);
                }


                foreach (var script in _repository.GetValueOrDefault(repository))
                {
                    try
                    {
                        _client.Cypher.Match("(r:ScriptRepository)", "(s:ScriptExecution)")
                        .Where((ScriptRepository r) => r.Name == repository.Name)
                        .AndWhere((ScriptExecution s) => s.ExecCommand == script.ExecCommand)
                        .Create("(s)-[:IS_ON]->(r)")
                        .ExecuteWithoutResultsAsync().Wait();
                        _logger.LogInformation("[{time}] Linking ScriptExecution {ScriptName} to ScriptRepository {name}  ...", DateTime.UtcNow, script.Name, repository.Name);
                    }
                    catch (Exception e)
                    {
                        _logger.LogInformation("[{time}] Could not link ScriptExecution {ScriptName} to ScriptRepository {name} ...", DateTime.UtcNow, script.Name, repository.Name);
                    }

                }
            }
            return Task.CompletedTask;


        }


    }
}
