namespace SteerMyWheel.Connectivity
{
    public interface IClientProvider<T>
    {
        T GetConnection();

        void Connect();
    }
}