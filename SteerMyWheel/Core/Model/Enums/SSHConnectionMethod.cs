namespace SteerMyWheel.Core.Model.Enums
{
    /// <summary>
    /// Represents the ssh connection mode to a RemoteHost
    /// </summary>
    public enum SSHConnectionMethod
    {
        DEFAULT = 0, // Username & password
        SSL = 1 // Private/Public key
    }
}
