
using Microsoft.SemanticKernel;

namespace Demo;

public class Basic00 : BaseDemo
{
    public override Kernel CreateKernel(SKConfig config)
    {
        var kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(config.AzureOpenAICompletionDeploymentName, config.AzureOpenAIEndpoint, config.AzureOpenAIKey)
            // .AddOpenAIChatCompletion(config.OpenAICompletionModelId, config.OpenAIKey)
            .Build();

        return kernel;
    }

    protected override async Task<string?> HandlePrompt(Kernel kernel, string userPrompt)
        => await kernel.InvokePromptAsync<string>(userPrompt);
}