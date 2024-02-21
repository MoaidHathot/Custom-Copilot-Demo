using Dumpify;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
using System.Text;

namespace Demo;

#pragma warning disable SKEXP0050

public class Local00LMStudio00 : BaseDemo
{
    private readonly ChatHistory _chatHistory = [];

    public override Kernel CreateKernel(SKConfig config)
    {
        var builder = Kernel.CreateBuilder()
            // .AddAzureOpenAIChatCompletion(config.AzureOpenAICompletionDeploymentName, config.AzureOpenAIEndpoint, config.AzureOpenAIKey)
            .AddOpenAIChatCompletion(modelId: "_", apiKey: "_", httpClient: new HttpClient(new HttpMessageInterceptorForLmStudio()))
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
        var settings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
        };

        var completionService = kernel.GetRequiredService<IChatCompletionService>();

        if (_chatHistory.Count > 10)
        {
            _chatHistory.RemoveRange(0, 5);
        }

        _chatHistory.AddUserMessage(userPrompt);

        var stream = completionService.GetStreamingChatMessageContentsAsync(_chatHistory, executionSettings: settings, kernel: kernel);

        var message = new StringBuilder();
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        await foreach (var content in stream)
        {
            message.Append(content.Content);
            Console.Write(content.Content);
        }

        Console.ForegroundColor = ConsoleColor.Gray;

        if (message.Length > 0)
        {
            _chatHistory.AddAssistantMessage(message.ToString());
        }

        return null;
    }

    public class HttpMessageInterceptorForLmStudio : HttpClientHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            "Intercepting call for LLM".Dump(colors: new ColorConfig { PropertyValueColor = "DarkGreen" });
            if (request.RequestUri != null && request.RequestUri.Host.Equals("api.openai.com", StringComparison.OrdinalIgnoreCase))
            {
                request.RequestUri = new Uri($"http://localhost:1234{request.RequestUri.PathAndQuery}");
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}