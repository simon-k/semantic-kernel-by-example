using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SemanticKernelByExample.Examples;
using Spectre.Console;

Console.WriteLine("Welcome to the Semantic Kernel Example Chat Bot");

// Create a HttpClient with certificate revocation list check disabled. Else it might fail on some systems using proxies like Zscaler
 var handler = new HttpClientHandler();
 handler.CheckCertificateRevocationList = false;
 var client = new HttpClient(handler);

var openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new Exception("Create an environment variable named 'OPENAI_API_KEY' with your OpenAI API key");

var kernelBuilder = Kernel.CreateBuilder();

/*kernelBuilder.Services.AddLogging(c =>
{
    c.AddConsole().SetMinimumLevel(LogLevel.Trace);
});*/

var kernel = kernelBuilder
    .AddOpenAIChatCompletion("gpt-4o", openAiApiKey, httpClient: client) 
    .Build();

AnsiConsole.Clear();
var example = AnsiConsole.Prompt(
    new SelectionPrompt<Example>()
        .Title("Select an example to run?")
        .PageSize(10)
        .MoreChoicesText("[grey](Move up and down to see more examples)[/]")
        .AddChoices(
            new StatelessChat(), 
            new StatefulChat(),
            new KernelFunctionChat(),
            new WebSearchChat(client),
            new WebsiteContentChat(),
            new MetaPromptChat(),
            new Multiagent(),
            new CodeInterpreter(openAiApiKey)
        ));
    
example.ExecuteAsync(kernel).Wait();    
