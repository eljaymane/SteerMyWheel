using SteerMyWheel.Domain.Model.Entity;

namespace SteerMyWheel.Core.Model.Entities
{
    public class ScriptRepository : BaseEntity<string>
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string LegacyRepository { get; set; }
        public string BitbucketRepository { get; set; }
        public bool IsCloned { get; set; }

        public ScriptRepository()
        {

        }
        public ScriptRepository(string path, string name)
        {
            Path = path;
            Name = name;
            LegacyRepository = "https://gitlab.keplercheuvreux.com/it-front/scripts/" + $"{name}.git";
            BitbucketRepository = $"https://bitbucket.org/kch-it-tet/{name}.git";
        }

        public override bool Equals(BaseEntity<string> other)
        {
            return Name == ((ScriptRepository)other).Name;
        }
    }
}
