using System.Text;
using Dumpify;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;

namespace Demo;

#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0011

public class Memory00Load01 : BaseDemo
{
    private readonly ChatHistory _chatHistory = [];

    public override Kernel CreateKernel(SKConfig config)
    {
        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(config.AzureOpenAICompletionDeploymentName, config.AzureOpenAIEndpoint, config.AzureOpenAIKey)
            // .AddOpenAIChatCompletion(config.OpenAICompletionModelId, config.OpenAIKey)
            ;

        builder.Plugins.AddFromType<TimePlugin>();
        builder.Plugins.AddFromType<DynamicMemoryLoaderPlugin>();

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
        await GetQuestionAnswer(KernelMemory, userPrompt);
        return null;
    }

    private async Task GetQuestionAnswer(IKernelMemory kernelMemory, string userPrompt)
    {
        var answer = await kernelMemory.AskAsync(userPrompt, minRelevance: 0.8);
        (answer.Result, answer.RelevantSources.Select(c => c.DocumentId)).Dump();
    }

    private async Task GetRelevantDocuments(IKernelMemory kernelMemory, string userPrompt)
    {
        var searchResult = await kernelMemory.SearchAsync(userPrompt, minRelevance: 0.8);
        foreach (var result in searchResult.Results)
        {
            result.DocumentId.Dump();
        }
    }
}