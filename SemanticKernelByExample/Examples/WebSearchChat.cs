using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Spectre.Console;

namespace SemanticKernelByExample.Examples;

/// <summary>
/// A web search enabled chat example. You can ask the chat questions and it will use Bing to look up the answer. 
/// 
/// Tell the bot "I am from Denmark" and ask it "How is the weather tomorrow?" and see how it responds. It will use Bing to look up the weather.
/// </summary>
public class WebSearchChat(HttpClient httpClient) : Example
{
    public override async Task ExecuteAsync(Kernel kernel)
    {
        #pragma warning disable SKEXP0050
        var bingConnectorKey = Environment.GetEnvironmentVariable("BING_CONNECTOR_KEY") ?? throw new Exception("Create an environment variable named 'BING_CONNECTOR_KEY' with your Bing API key");
        
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        kernel.ImportPluginFromObject(new WebSearchEnginePlugin(new BingConnector(bingConnectorKey, httpClient)));    // Import the web search plugin. The httpClient with CheckCertificateRevocationList=faklse is needed because of ZScaler or other proxy. You might be able to make it work without if you are not on a corporate network  
        var settings = new OpenAIPromptExecutionSettings() {ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions}; // Tell the kernel to invoke functions by itself
        var chatHistory = new ChatHistory();
        
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("This is a [bold][italic]web search enabled chat[/][/]. It will remember the context of the conversation and use Bing to look up questions if needed. Type [yellow]exit[/] to return to the main menu.");
        while (true)
        {
            var question = AnsiConsole.Ask<string>("[purple]Q?[/]");
            if (question == "exit") break; 
            
            chatHistory.AddUserMessage(question);
            var answer = await chatService.GetChatMessageContentAsync(chatHistory, settings, kernel);
            chatHistory.Add(answer);
            
            AnsiConsole.MarkupLine($"[orange1]A:[/] {answer}");
        }
        #pragma warning restore SKEXP0050
    }

    public override string ToString()
    {
        return "Web Search Chat Example";
    }
}