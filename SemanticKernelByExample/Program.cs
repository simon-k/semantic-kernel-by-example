using HtmlAgilityPack;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;

Console.WriteLine("Welcome to the Semantic Kernel Example Chat Bot");

var openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new Exception("Create an environment variable named 'OPENAI_API_KEY' with your OpenAI API key");
var bingConnectorKey = Environment.GetEnvironmentVariable("BING_CONNECTOR_KEY") ?? throw new Exception("Create an environment variable named 'BING_CONNECTOR_KEY' with your Bing API key");

// Create a HttpClient with certificate revocation list check disabled. Else it might fail on some systems using proxies like Zscaler
 var handler = new HttpClientHandler();
 handler.CheckCertificateRevocationList = false;
 var client = new HttpClient(handler);

var kernelBuilder = Kernel.CreateBuilder(); 
var kernel = kernelBuilder
    .AddOpenAIChatCompletion("gpt-4", openAiApiKey, httpClient: client) 
    .Build();
 
// Step 1: Stateless Chat
// ======================
// Try to ask the bot "Tell me a taco joke" or "give me a taco recipe" and see how it responds
// Try to tell the bot "My name is Simon" and "What is my name?" and see how it responds
/*
while (true)
{
    Console.Write("Q: ");
    var question = Console.ReadLine();
    var answer = await kernel.InvokePromptAsync(question);
    Console.WriteLine($"A: {answer}");
}
*/

// Step 2: Stateful Chat
// =====================
// Try to ask the bot "What is your name?" and "What is my name?" and see how it responds
// Tell the bot "I am from Denmark" and ask it "What is my favorite food?" and see how it responds
/*var chatService = kernel.GetRequiredService<IChatCompletionService>();
var chatHistory = new ChatHistory();
while (true)
{
    Console.Write("Q: ");
    chatHistory.AddUserMessage(Console.ReadLine());
    var answer = await chatService.GetChatMessageContentAsync(chatHistory);
    Console.WriteLine($"A: {answer}");
    chatHistory.Add(answer);
}*/

// Step 3: Stateful Chat with Kernel Function
// ==========================================
// Tell the bot "I am from Denmark" and ask it "What is my favorite food?" and see how it responds
// Ask the bot "How is the weather tomorrow?" and see how it responds
/*kernel.ImportPluginFromType<Demographics>();                                // Import the demographics as a plugin to the kernel
var settings = new OpenAIPromptExecutionSettings() {ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions}; // Tell the kernel to invoke functions by itself
var chatService = kernel.GetRequiredService<IChatCompletionService>();
var chatHistory = new ChatHistory();
while (true)
{
    Console.Write("Q: ");
    chatHistory.AddUserMessage(Console.ReadLine());
    var answer = await chatService.GetChatMessageContentAsync(chatHistory, settings, kernel);
    Console.WriteLine($"A: {answer}");
    chatHistory.Add(answer);
}

class Demographics
{
    [KernelFunction]    //Import and allow kernel to invoke this function
    public string GetFavoriteFood(string country)
    {
        return country switch
        {
            "Denmark" => "Frikadeller",
            "USA" => "Hotdogs",
            "Nexico" => "Tacos",
            _ => "Unknown"
        };
    }
}*/

// Step 4: Stateful Chat with Binge Search Plugin
// ==============================================
// Tell the bot "I am from Denmark" and ask it "How is the weather tomorrow?" and see how it responds
/*
#pragma warning disable SKEXP0050
var chatService = kernel.GetRequiredService<IChatCompletionService>();
kernel.ImportPluginFromObject(new WebSearchEnginePlugin(new BingConnector(bingConnectorKey)));    // Import the web search plugin 
var settings = new OpenAIPromptExecutionSettings() {ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions}; // Tell the kernel to invoke functions by itself
var chatHistory = new ChatHistory();
while (true)
{
    Console.Write("Q: ");
    chatHistory.AddUserMessage(Console.ReadLine()!);
    var answer = await chatService.GetChatMessageContentAsync(chatHistory, settings, kernel);
    Console.WriteLine($"A: {answer}");
    chatHistory.Add(answer);
}
#pragma warning restore SKEXP0050
*/


// Step 5: Get content from a website
// ==============================================
// Ask the bot "What is on the menu on Tuesday?" and "What are the opening hours on sunday?" and see how it responds
// You could also ask it for "a recipe for the menu on Wednesday"
kernel.ImportPluginFromType<GetCanteenPlugin>(); 
var settings = new OpenAIPromptExecutionSettings() {ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions}; // Tell the kernel to invoke functions by itself
var chatService = kernel.GetRequiredService<IChatCompletionService>();
var chatHistory = new ChatHistory();
while (true)
{
    Console.Write("Q: ");
    chatHistory.AddUserMessage(Console.ReadLine());
    var answer = await chatService.GetChatMessageContentAsync(chatHistory, settings, kernel);
    Console.WriteLine($"A: {answer}");
    chatHistory.Add(answer);
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
