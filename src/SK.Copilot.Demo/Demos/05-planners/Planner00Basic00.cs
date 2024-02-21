using Dumpify;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Planning.Handlebars;
using Microsoft.SemanticKernel.Plugins.Core;
using System.Text;

namespace Demo;

#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0060
#pragma warning disable SKEXP0011

public class Planner00Basic00 : BaseDemo
{
    private HandlebarsPlanner _planner = new HandlebarsPlanner(new HandlebarsPlannerOptions());


    public override Kernel CreateKernel(SKConfig config)
    {
        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(config.AzureOpenAICompletionDeploymentName, config.AzureOpenAIEndpoint, config.AzureOpenAIKey)
            // .AddOpenAIChatCompletion(config.OpenAICompletionModelId, config.OpenAIKey)
            ;

        builder.Plugins.AddFromType<TimePlugin>();
        builder.Plugins.AddFromType<DynamicMemoryLoaderPlugin>();
        builder.Plugins.AddFromType<GithubPlugin>();

        builder.Services.AddSingleton(KernelMemory);

        return builder.Build();
    }

    public override async Task<IKernelMemory?> CreateMemoryAsync(SKConfig config)
    {
        var memory = new KernelMemoryBuilder()
            .WithAzureOpenAITextGeneration(new AzureOpenAIConfig { APIKey = config.AzureOpenAIKey, Endpoint = config.AzureOpenAIEndpoint, Deployment = config.AzureOpenAICompletionDeploymentName, Auth = AzureOpenAIConfig.AuthTypes.APIKey })
            .WithAzureOpenAITextEmbeddingGeneration(new AzureOpenAIConfig { APIKey = config.AzureOpenAIKey, Endpoint = config.AzureOpenAIEndpoint, Deployment = config.AzureOpenAIEmbeddingDeploymentName, Auth = AzureOpenAIConfig.AuthTypes.APIKey })
            .Build();

        var docLocation = Path.Combine(Directory.GetCurrentDirectory(), "SK.Copilot.Demo/Memories");
        var tasks = Directory
            .GetFiles(docLocation)
            .Select(f => new FileInfo(f))
            .Select(info => (info, text: File.ReadAllText(info.FullName)))
            .Select(tuple => memory.ImportDocumentAsync(tuple.info.FullName, documentId: tuple.info.Name));

        await Task.WhenAll(tasks);

        return memory;
    }

    public override Task<KernelPlugin[]> CreatePluginsAsync(Kernel kernel)
    {
        KernelPlugin[] plugins = [
            kernel.CreatePluginFromPromptDirectory("SK.Copilot.Demo/Prompts"),
        ];

        return Task.FromResult(plugins);
    }

    public override string ScreenPrompt => "What do you want to know about time?";

    protected override async Task<string?> HandlePrompt(Kernel kernel, string userPrompt)
    {
        "Creating plan".Dump(colors: new ColorConfig { PropertyValueColor = "Purple" });
        var plan = await _planner.CreatePlanAsync(kernel, userPrompt);
        plan.ToString().Dump(colors: new ColorConfig { PropertyValueColor = "Green" });
        "Executing plan".Dump(colors: new ColorConfig { PropertyValueColor = "Purple" });
        var result = await plan.InvokeAsync(kernel);

        return result;
    }
}