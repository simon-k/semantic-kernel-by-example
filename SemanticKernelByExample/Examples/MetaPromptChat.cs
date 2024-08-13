using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Spectre.Console;

namespace SemanticKernelByExample.Examples;

public class MetaPromptChat : Example
{
    public override async Task ExecuteAsync(Kernel kernel)
    {
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var chatHistory = new ChatHistory("You are a extremely friendly and joking assistant that only knows about tacos recipes. Every time you answer a question you tell a taco joke");
        
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("This is a [bold][italic]metaprompt chat example[/][/]. It will be instructed to be friendly, joking and only know about taco recipes. Type [yellow]exit[/] to return to the main menu.");
        while (true)
        {
            var question = AnsiConsole.Ask<string>("[purple]Q?[/]");
            if (question == "exit") break;    
            
            chatHistory.AddUserMessage(question);
            var answer = await chatService.GetChatMessageContentAsync(chatHistory);
            chatHistory.Add(answer);
            
            AnsiConsole.MarkupLine($"[orange1]A:[/] {answer}");
        }
    }

    public override string ToString()
    {
        return "Meta prompt example";
    }
}