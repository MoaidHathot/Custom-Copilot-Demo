using Microsoft.SemanticKernel;

namespace Demo;

public class Prompt00Order00 : BaseDemo
{
    public override Kernel CreateKernel(SKConfig config)
    {
        var kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(config.AzureOpenAICompletionDeploymentName, config.AzureOpenAIEndpoint, config.AzureOpenAIKey)
            // .AddOpenAIChatCompletion(config.OpenAICompletionModelId, config.OpenAIKey)
            .Build();

        return kernel;
    }

    public override Task<KernelPlugin[]> CreatePluginsAsync(Kernel kernel)
    {
        KernelPlugin[] plugins = [kernel.CreatePluginFromPromptDirectory("SK.Copilot.Demo/Prompts/")];

        return Task.FromResult(plugins);
    }

    public override string ScreenPrompt => "What do you want to order?";

    protected override async Task<string?> HandlePrompt(Kernel kernel, string userPrompt)
    {
        var args = new KernelArguments
        {
            ["order"] = userPrompt,
        };

        var plugin = Plugins["Prompts"]!["Order"];

        return await kernel.InvokeAsync<string>(plugin, args);
    }
}