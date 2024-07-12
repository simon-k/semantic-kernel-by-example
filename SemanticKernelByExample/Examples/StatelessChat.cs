using Microsoft.SemanticKernel;
using Spectre.Console;

namespace SemanticKernelByExample.Examples;

/// <summary>
/// A stateless chat example. You can ask the chat questions but it will not remember the context of the conversation.
///
/// Try to ask the bot "Tell me a taco joke" or "give me a taco recipe" and see how it responds.
/// Try to tell the bot "My name is Simon" and "What is my name?" and see how it responds. You'll see that it does not remember your name
/// </summary>
public class StatelessChat : Example
{
    public override async Task ExecuteAsync(Kernel kernel)
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("This is a [bold][italic]stateless chat example[/][/]. It will not remember the context of the conversation. Type [yellow]exit[/] to return to the main menu.");
        while (true)
        {
            var question = AnsiConsole.Ask<string>("[purple]Q?[/]");
            if (question == "exit") break;            
            
            var answer = await kernel.InvokePromptAsync(question);
            
            AnsiConsole.MarkupLine($"[orange1]A:[/] {answer}");
        }
    }
    
    public override string ToString()
    {
        return "Stateless Chat Example";
    }
}