using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Spectre.Console;

namespace SemanticKernelByExample.Examples;
/// <summary>
/// This is a statefull chat with a Kernel Function that can be invoked by the kernel to derive information based on some logic.
///  
/// Tell the bot "I am from Denmark" and ask it "What is my favorite food?" and see how it responds. Based on the country you are from, the bot will derive your favorite food.
/// Ask the bot "How is the weather tomorrow?" and see how it responds. It cannot respond because it does not have a way to look up the weather.
/// </summary>
public class KernelFunctionChat : Example
{
    public override async Task ExecuteAsync(Kernel kernel)
    {
        kernel.ImportPluginFromType<Demographics>();                                // Import Demographics as a plugin to the kernel
        var settings = new OpenAIPromptExecutionSettings() {ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions}; // Tell the kernel to invoke functions by itself
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var chatHistory = new ChatHistory();
        
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("This is a [bold][italic]stateful chat with kernel function example[/][/]. It will remember the context of the conversation and use a kernel function to derive a response. Type [yellow]exit[/] to return to the main menu.");
        while (true)
        {
            var question = AnsiConsole.Ask<string>("[purple]Q?[/]");
            if (question == "exit") break;    
            
            chatHistory.AddUserMessage(question);
            var answer = await chatService.GetChatMessageContentAsync(chatHistory, settings, kernel);
            chatHistory.Add(answer);
            
            AnsiConsole.MarkupLine($"[orange1]A:[/] {answer}");
        }
    } 
    
    public override string ToString()
    {
        return "Kernel Function Chat Example";
    }

    class Demographics
    {
        [KernelFunction]
        [Description("Gets the favorite food based on the country you are from")]
        public string GetFavoriteFood(string country)
        {
            return country switch
            {
                "Denmark" => "Frikadeller",
                "USA" => "Hotdogs",
                "Mexico" => "Tacos",
                _ => "Unknown"
            };
        }
    }
}