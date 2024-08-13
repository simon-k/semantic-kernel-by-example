using Azure.AI.OpenAI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(
        deploymentName: Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME"),
        endpoint: Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT"),
        apiKey: Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY"))
    .Build();

Console.WriteLine("Ask a question before the Azure AI Search extension is loaded");
Console.Write("Q: ");
var question = Console.ReadLine();
var result = await kernel.InvokePromptAsync(question);
Console.WriteLine($"A: {result}");

#pragma warning disable SKEXP0010
var azureSearchExtensionConfiguration = new AzureSearchChatExtensionConfiguration
{
    SearchEndpoint = new Uri(Environment.GetEnvironmentVariable("AZURE_AI_SEARCH_ENDPOINT")),
    Authentication = new OnYourDataApiKeyAuthenticationOptions(Environment.GetEnvironmentVariable("AZURE_AI_SEARCH_API_KEY")),
    IndexName = Environment.GetEnvironmentVariable("AZURE_AI_SEARCH_INDEX")
};

var chatExtensionsOptions = new AzureChatExtensionsOptions { Extensions = { azureSearchExtensionConfiguration } };
var executionSettings = new OpenAIPromptExecutionSettings { AzureChatExtensionsOptions = chatExtensionsOptions };

Console.WriteLine("Ask a question after the Azure AI Search extension is loaded");
Console.Write("Q: ");
question = Console.ReadLine();
result = await kernel.InvokePromptAsync(question, new(executionSettings));
Console.WriteLine($"A: {result}");