using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
using OpenAI.Chat;
using Spectre.Console;

namespace SemanticKernelByExample.Examples;
/// <summary>
/// A chat example with the HTTP plugin. Try to ask it a question about a website. Like "Given the canteen website https://madkastel.dk/hubnordic/ what is the menu on monday?"
/// </summary>
public class HttpPluginChat : Example
{
    [Experimental("SKEXP0050")]
    public override async Task ExecuteAsync(Kernel kernel)
    {
        var handler = new HttpClientHandler();
        handler.CheckCertificateRevocationList = false;
        var client = new HttpClient(handler);

        var httpPlugin = new HttpPlugin(client);

        kernel.ImportPluginFromObject(httpPlugin);
        var settings = new OpenAIPromptExecutionSettings()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var chatHistory = new ChatHistory("Reply with Markdown");
        
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("This is a [bold][italic]chat with the HTTP plugin[/][/]. Try to ask it a question about a website. Like [purple]Given the canteen website https://madkastel.dk/hubnordic/ what is the menu on monday?[/]");
        while (true)
        {
            var question = AnsiConsole.Ask<string>("[purple]> [/]");
            
            chatHistory.AddUserMessage(question);
            var answer = await chatService.GetChatMessageContentAsync(chatHistory, settings, kernel);
            chatHistory.Add(answer);
            
            AnsiConsole.MarkupLine($"[orange1]{answer}[/]");
        }
    } 
    
    public override string ToString()
    {
        return "Http Plugin Chat Example";
    }

}