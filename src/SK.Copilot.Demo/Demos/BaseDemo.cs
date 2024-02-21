
using Dumpify;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using System.Drawing;

namespace Demo;

public abstract class BaseDemo : IDemo
{
    private Kernel? _kernel;
    private Dictionary<string, KernelPlugin> _plugins = [];

    public Dictionary<string, KernelPlugin> Plugins => _plugins;

    private IKernelMemory? _memory;
    public IKernelMemory KernelMemory => _memory!;

    public BaseDemo()
    {
        GetType().Name.Dump(colors: new ColorConfig { PropertyValueColor = "Orange" });
    }

    public async Task InitializeAsync(SKConfig config)
    {
        _memory = await CreateMemoryAsync(config);
        _kernel = CreateKernel(config);
        _plugins = (await CreatePluginsAsync(_kernel!)).ToDictionary(p => p.Name);
    }

    public async Task RunAsync(SKConfig config)
    {
        while (true)
        {
            ScreenPrompt.Dump(colors: new ColorConfig { PropertyValueColor = Color.Aqua });
            var query = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(query))
            {
                continue;
            }

            var result = await HandlePrompt(_kernel!, query);

            if (result is null)
            {
                Console.WriteLine();
            }
            else
            {
                result.Dump();
            }
        }
    }

    public virtual string ScreenPrompt => "How can I help you?";

    public abstract Kernel CreateKernel(SKConfig config);
    protected abstract Task<string?> HandlePrompt(Kernel kernel, string userPrompt);

    public virtual Task<KernelPlugin[]> CreatePluginsAsync(Kernel kernel)
        => Task.FromResult<KernelPlugin[]>([]);

    public virtual Task<IKernelMemory?> CreateMemoryAsync(SKConfig config)
        => Task.FromResult<IKernelMemory?>(null);

    public ValueTask DisposeAsync()
        => ValueTask.CompletedTask;
}