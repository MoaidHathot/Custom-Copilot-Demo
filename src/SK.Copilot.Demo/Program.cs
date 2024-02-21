using Demo;

var config = new SKConfig
{
    // AzureOpenAICompletionDeploymentName = "gpt-35-turbo-16k",
    AzureOpenAICompletionDeploymentName = "gpt-4-0125-Preview",
    AzureOpenAIEmbeddingDeploymentName = "text-embedding-ada-002",
    AzureOpenAIKey = Environment.GetEnvironmentVariable("demo_aoi_key")!,
    AzureOpenAIEndpoint = Environment.GetEnvironmentVariable("demo_aoi_endpoint")!,

    OpenAIKey = Environment.GetEnvironmentVariable("demo_oi_key")!,
    OpenAICompletionModelId = "gpt-4-turbo-preview",
};

await using var demo = DemoUtils.GetDemo(args[0]);

await demo.InitializeAsync(config);
await demo.RunAsync(config);