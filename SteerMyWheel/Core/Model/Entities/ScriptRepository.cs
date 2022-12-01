namespace SteerMyWheel.Core.Model.Entities
{
    /// <summary>
    /// Represents a git repository that corresponds to a script execution executable.
    /// </summary>
    public class ScriptRepository : BaseEntity<string>
    {
        /// <summary>
        /// The path of the repository in a RemoteHost
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// Name of the repository
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Legacy repository link 
        /// </summary>
        public string LegacyRepository { get; set; }
        /// <summary>
        /// Bitbucket repository link
        /// </summary>
        public string BitbucketRepository { get; set; }
        /// <summary>
        /// If the repository has been successfully cloned
        /// </summary>
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

        public override string GetID()
        {
            return Name;
        }
    }
}
