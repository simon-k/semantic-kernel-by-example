using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Spectre.Console;

namespace SemanticKernelByExample.Examples;

/// <summary>
/// A stateful chat example. You can ask the chat questions and it will remember the context of the conversation.
///
/// A stateful chat is achieved by using the ChatHistory class to keep track of the conversation history.
///
/// Try to tell the bot "My name is Simon" and "What is my name?" and see how it responds. You'll see that it does not remember your name
/// Tell the bot "I am from Denmark" and ask it "What is my favorite food?" and see how it responds. You'll see that it cannot derive your favorite food based on where you are from.
/// </summary>
public class StatefulChat : Example
{
    public override async Task ExecuteAsync(Kernel kernel)
    {
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var chatHistory = new ChatHistory();
        
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("This is a [bold][italic]stateful chat example[/][/]. It will remember the context of the conversation.");
        while (true)
        {
            var question = AnsiConsole.Ask<string>("[purple]Q?[/]");
            
            chatHistory.AddUserMessage(question);
            var answer = await chatService.GetChatMessageContentAsync(chatHistory);
            chatHistory.Add(answer);
            
            AnsiConsole.MarkupLine($"[orange1]A:[/] {answer}");
        }
    }
    
    public override string ToString()
    {
        return "Stateful Chat Example";
    }
}