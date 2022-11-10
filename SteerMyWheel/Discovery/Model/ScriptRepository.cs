using SteerMyWheel.Writer;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace SteerMyWheel.Discovery.Model
{
    public class ScriptRepository : IWritable, IEquatable<ScriptRepository>
    {
        public string path { get; set; }
        public string name { get; set; }
        public string legacyRepository { get; set; }
        public string BitbucketRepository { get; set; }

        public ScriptRepository(string path, string name)
        {
            this.path = path;
            this.name = name;
            this.legacyRepository = "https://gitlab.keplercheuvreux.com/it-front/scripts/" + $"{name}.git";
            BitbucketRepository = $"https://bitbucket.org/kch-it-tet/{name}.git";
        }

        public string CreateQuery()
        {
            throw new NotImplementedException();
        }

        public bool Equals([AllowNull] ScriptRepository other)
        {
            return other.name == this.name;
        }
    }
}
