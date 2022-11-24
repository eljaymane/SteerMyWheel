using Microsoft.Extensions.Configuration;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace SteerMyWheel.Configuration
{
    public class GlobalConfig
    {
        public GlobalConfig()
        {

        }
        private IConfiguration _config { get; }
        public string bitbucketUsername { get { return _config["Bitbucket:Login"]; } }
        public string bitbucketPassword { get { return _config["Bitbucket:Password"]; } }
        public string neo4jRootURI { get { return _config["Neo4j:RootURI"]; } }
        public string neo4jUsername { get { return _config["Neo4j:Username"]; } }
        public string neo4jPassword { get { return _config["Neo4j:Password"]; } }
        public string neo4jDefaultDB { get { return _config["Neo4j:DefaultDB"]; } }
        public string gitLabScriptsBaseURI { get { return "https://gitlab.keplercheuvreux.com/it-front/scripts/"; } }
        public string gitLabCommandoBaseURI { get { return "https://gitlab.keplercheuvreux.com/it-front/commando/"; } }
        public string bitbucketSecret { get { return _config["Bitbucket:Secret"]; } }
        public string bitbucketKey { get { return _config["Bitbucket:Key"]; } }
        public string bitbucketScriptsProject { get { return _config["Bitbucket:ScriptsProject"]; } }
        public string bitbucketAccessTokenURI { get { return "https://bitbucket.org/site/oauth2/access_token"; } }
        public string bitbucketCodeURI { get { return $"https://bitbucket.org/site/oauth2/authorize?client_id={bitbucketKey}&response_type=code"; } }
        public string bitbucketScriptsAPI { get { return $"https://api.bitbucket.org/2.0/repositories/kch-it-tet/"; } }
        public string SSHKeysPATH { get { return $"{LocalWorkingDirectory}/.ssh/"; } }
        public string LocalWorkingDirectory { get { return @"C:\steer\"; } }

        public string LocalReposDirectory { get { return LocalWorkingDirectory + @"repos\"; } }
        public string DefaultCommitMessage { get { return "[Automigration] Updated by SteerMyWheel"; } }
        public string DefaultRemoteName { get { return "origin"; } }
        public GlobalConfig(IConfiguration config)
        {
            _config = config;
        }



    }
}
