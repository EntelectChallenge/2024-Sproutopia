namespace Logger
{
    public interface IStreamingFileLogger
    {
        Task Log(object state);

        Task Close();
    }
}
