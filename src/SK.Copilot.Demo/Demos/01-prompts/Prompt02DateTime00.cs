using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;

namespace Demo;

#pragma warning disable SKEXP0050

public class Prompt02DateTime00 : BaseDemo
{
    public override Kernel CreateKernel(SKConfig config)
    {
        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(config.AzureOpenAICompletionDeploymentName, config.AzureOpenAIEndpoint, config.AzureOpenAIKey)
            // .AddOpenAIChatCompletion(config.OpenAICompletionModelId, config.OpenAIKey)
            ;

        builder.Plugins.AddFromType<TimePlugin>();

        return builder.Build();
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
        var args = new KernelArguments
        {
            ["input"] = userPrompt,
        };

        var plugin = Plugins["Prompts"]["TimeCalculator"];

        return await kernel.InvokeAsync<string>(plugin, args);
    }
}