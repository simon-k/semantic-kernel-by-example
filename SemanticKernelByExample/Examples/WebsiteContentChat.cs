using HtmlAgilityPack;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace SemanticKernelByExample.Examples;

/// <summary>
/// Get content from a website.
///
/// This is done by making a plugin with Kernel Functions that assist with getting the content from the website. The LLM can understand HTML and JSON content.
/// 
/// Ask the bot "What is on the menu on Tuesday?" and "What are the opening hours on sunday?" and see how it responds based on html pages.
/// Also ask "Who is the floor manager" and it will be looked up in a json document. 
/// You could also ask it for "a recipe for the menu on Wednesday"
/// </summary>
public class WebsiteContentChat : Example
{
    public override async Task ExecuteAsync(Kernel kernel)
    {
        kernel.ImportPluginFromType<GetCanteenPlugin>(); 
        var settings = new OpenAIPromptExecutionSettings {ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions}; // Tell the kernel to invoke functions by itself
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var chatHistory = new ChatHistory();
        while (true)
        {
            Console.Write("Q: ");
            chatHistory.AddUserMessage(Console.ReadLine()!);
            var answer = await chatService.GetChatMessageContentAsync(chatHistory, settings, kernel);
            Console.WriteLine($"A: {answer}");
            chatHistory.Add(answer);
        }
    }

    public override string ToString()
    {
        return "Website Content Chat Example";
    }
    
    class GetCanteenPlugin
    {
        [KernelFunction]
        public string GetMenu() {
            var htmlDoc = new HtmlDocument();
            htmlDoc.Load("CanteenWebsite/Menu.html");
            return htmlDoc.Text;
        }

        [KernelFunction]
        public string GetOpeningHours() {
            var htmlDoc = new HtmlDocument();
            htmlDoc.Load("CanteenWebsite/OpeningHours.html");
            return htmlDoc.Text;
        }
    
        [KernelFunction]
        public string GetStaff() {
            var staff = File.ReadAllText("CanteenWebsite/Staff.json");
            return staff;
        }
    }
}