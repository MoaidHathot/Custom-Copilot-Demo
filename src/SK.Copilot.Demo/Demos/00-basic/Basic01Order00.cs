
using Microsoft.SemanticKernel;

namespace Demo;

public class Basic01Order00 : BaseDemo
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
    {
        var calculatedPrompt = GetPromptVersion2(userPrompt);

        return await kernel.InvokePromptAsync<string>(calculatedPrompt);
    }

    public override string ScreenPrompt => "What do you want to order?";

    private string GetPromptVersion2(string userPrompt)
    => $"""
            What product is ordered in this request? {userPrompt}
            A product can be Coffee, Burger, Water, Shawarma, Shintzel, Beer.
         """;

}