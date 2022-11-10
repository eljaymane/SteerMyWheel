using Microsoft.Extensions.Configuration;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace SteerMyWheel
{
    public class GlobalConfig
    {
        private IConfiguration _config { get; }
        public string bitbucketUsername { get { return _config["Bitbucket:Login"]; } }
        public string bitbucketPassword { get { return _config["Bitbucket:Password"]; } }

        public string neo4jRootURI { get { return _config["Neo4j:RootURI"]; } }
        public string neo4jUsername { get { return _config["Neo4j:Username"]; } }
        public string neo4jPassword { get { return _config["Neo4j:Password"]; } }
        public string neo4jDefaultDB { get { return _config["Neo4j:DefaultDB"]; } }
        public string gitLabScriptsBaseURI { get { return "https://gitlab.keplercheuvreux.com/it-front/scripts/"; } }
        public string gitLabCommandoBaseURI { get { return "https://gitlab.keplercheuvreux.com/it-front/commando/"; } }

        public GlobalConfig(IConfiguration config)
        {
            _config = config;
        }



    }
}
