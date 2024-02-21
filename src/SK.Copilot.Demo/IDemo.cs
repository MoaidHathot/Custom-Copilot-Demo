namespace Demo;

public interface IDemo : IAsyncDisposable
{
    Task InitializeAsync(SKConfig config);
    Task RunAsync(SKConfig config);
}