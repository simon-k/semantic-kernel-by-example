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

/// </summary>
public class StructuredResponseChat : Example
{
    [Experimental("SKEXP0050")]
    public override async Task ExecuteAsync(Kernel kernel)
    {
        var handler = new HttpClientHandler();
        handler.CheckCertificateRevocationList = false;
        var client = new HttpClient(handler);

        var httpPlugin = new HttpPlugin(client);

        kernel.ImportPluginFromObject(httpPlugin);
        ChatResponseFormat chatResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
            jsonSchemaFormatName: "canteen_result",
            jsonSchema: BinaryData.FromString("""
                                              {
                                                  "type": "object",
                                                  "properties": {
                                                      "Weekdays": {
                                                          "type": "array",
                                                          "items": {
                                                              "type": "object",
                                                              "properties": {
                                                                  "Day": { "type": "string" },
                                                                  "MainDish": { "type": "string" },
                                                                  "Vegetarian": { "type": "string" }
                                                              },
                                                              "required": ["Day", "MainDish", "Vegetarian"],
                                                              "additionalProperties": false
                                                          }
                                                      }
                                                  },
                                                  "required": ["Weekdays"],
                                                  "additionalProperties": false
                                              }
                                              """),
            jsonSchemaIsStrict: true);
        
        var settings = new OpenAIPromptExecutionSettings()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            ResponseFormat = chatResponseFormat
        };
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var chatHistory = new ChatHistory();
        
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("This is a [bold][italic]chat with the HTTP plugin[/][/]. Try to ask it a question about a website. Like [purple]Get the HUB1 menu from https://madkastel.dk/hubnordic/ [/]");
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
        return "Structured Response Chat Example";
    }

}