using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.Agents.History;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Spectre.Console;

namespace SemanticKernelByExample.Examples;

#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0001


/// <summary>
/// A multiagent chat example. The chat is a group chat with a presenter, a comedian, and an audience.
///
/// Based on https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/examples/example-agent-collaboration?pivots=programming-language-csharp
/// </summary>
public class Multiagent : Example
{
    const string PresenterName = "Presenter";
    const string ComedianName = "Comedian";
    const string AudienceName = "Audience";
    
    public override async Task ExecuteAsync(Kernel kernel)
    {
        var comedianKernel = kernel.Clone();
        var presenterKernel = kernel.Clone();
        
        // Create an agent
        var comedianAgent =
            new ChatCompletionAgent
            {
                Name = ComedianName,
                Instructions =
                    """
                    You are a comedian that tells jokes. Your goal is to make the audience laugh.
                    
                    RULES:
                    - Keep telling a new joke every time it is your turn. Don't stop the show.
                    - If the audience boo, make a drum roll.
                    - If the joke is a question, answer the question yourself.
                    """,
                Kernel = comedianKernel,
                Arguments = new KernelArguments(new AzureOpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()/*, Temperature = 0.4d*/})
            };
        
        // Create another agent
        var audienceAgent =
            new ChatCompletionAgent
            {
                Name = AudienceName,
                Instructions =
                    """
                    You are the audience at a comedy club. Your goal is to enjoy the show. You don't like 20% of the jokes.

                    - If you like the joke, laugh.
                    - If you don't like the joke, boo.
                    """,
                Kernel = kernel,
                Arguments = new KernelArguments(new AzureOpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(), Temperature = 0.2d})
            };
        
        var presenterAgent =
            new ChatCompletionAgent
            {
                Name = PresenterName,
                Instructions =
                    """
                    You are the host at a comedy club. Your job is to present the comedian.
                    """,
                Kernel = presenterKernel,
            };
        
        // Create the group chat selection instruction
        var selectionFunction =
            AgentGroupChat.CreatePromptFunctionForStrategy(
                $$$"""
                    Examine the provided RESPONSE and choose the next participant.
                    State only the name of the chosen participant without explanation.
                    Never choose the participant named in the RESPONSE.

                    Choose only from these participants:
                    - {{{PresenterName}}}
                    - {{{ComedianName}}}
                    - {{{AudienceName}}}

                    Always follow these rules when choosing the next participant:
                    - If RESPONSE is user input, it is {{{PresenterName}}}'s turn.
                    - If RESPONSE is by {{{PresenterName}}}, it is {{{ComedianName}}}'s turn.
                    - If RESPONSE is by {{{ComedianName}}}, it is {{{AudienceName}}}'s turn.
                    - If RESPONSE is by {{{AudienceName}}}, it is {{{ComedianName}}}'s turn.

                    RESPONSE:
                    {{$lastmessage}}
                    """,
                safeParameterNames: "lastmessage");
        
        // Create the group chat termination instruction
        const string TerminationToken = "TERMINATE";
        var terminationFunction =
            AgentGroupChat.CreatePromptFunctionForStrategy(
                $$$"""
                   Examine the RESPONSE. Only if the {{{AudienceName}}} booed respond with the word {{{TerminationToken}}}.
                   If the {{{AudienceName}}} laughed, continue the conversation by responding with CONTINUE.

                   RESPONSE:
                   {{$lastmessage}}
                   """,
                safeParameterNames: "lastmessage");
        
        var historyReducer = new ChatHistoryTruncationReducer(1);   //TODO: Try to increment this and tell the termination function to terminate if the audience booed twice in a row
        
        var chat =
            new AgentGroupChat(comedianAgent, audienceAgent)
            {
                ExecutionSettings = new AgentGroupChatSettings
                {
                    SelectionStrategy =
                        new KernelFunctionSelectionStrategy(selectionFunction, kernel)
                        {
                            InitialAgent = presenterAgent,               // Always start with the comedian
                            HistoryReducer = historyReducer,            
                            HistoryVariableName = "lastmessage",        // The prompt variable name for the history argument.
                            ResultParser = (result) => result.GetValue<string>() ?? comedianAgent.Name      // Returns the entire result value as a string.
                        },
                    TerminationStrategy =
                        new KernelFunctionTerminationStrategy(terminationFunction, kernel)
                        {
                            Agents = [audienceAgent],               // Only evaluate for editor's response
                            HistoryReducer = historyReducer,        // Save tokens by limit responses
                            HistoryVariableName = "lastmessage",    // The prompt variable name for the history argument.
                            MaximumIterations = 20,                 // Limit total number of turns
                            ResultParser = (result) =>
                            {
                                //Console.WriteLine($"Evaluation result: {result.GetValue<string>()}");
                                return result.GetValue<string>()
                                           ?.Contains(TerminationToken, StringComparison.OrdinalIgnoreCase) ??
                                       false; // Customer result parser to determine if the response is "yes"
                            }
                        }
                }
            };
        
        Console.WriteLine("Ready!");
        
        while(true)
        {
            Console.WriteLine("*** START A NEW SHOW ***");
            Console.Write("> ");
            string input = Console.ReadLine();
            /*if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }*/
            //input = input.Trim();

            //if (input.Equals("RESET", StringComparison.OrdinalIgnoreCase))
            //{
                await chat.ResetAsync();        //TODO: Maybe always reset chat before starting a new show?
            //    Console.WriteLine("[Converation has been reset]");
            //    continue;
            //}

            chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, input));
            chat.IsComplete = false;

            try
            {
                await foreach (ChatMessageContent response in chat.InvokeAsync())
                {
                    Console.WriteLine();
                    var color = GetAgentColor(response.AuthorName);
                    AnsiConsole.MarkupLine($"[{color}]{response.AuthorName.ToUpperInvariant()}:{Environment.NewLine}{response.Content.Replace('[', '{').Replace(']', '}')}[/]");
                }
            }
            catch (HttpOperationException exception)
            {
                Console.WriteLine(exception.Message);
                if (exception.InnerException != null)
                {
                    Console.WriteLine(exception.InnerException.Message);
                    if (exception.InnerException.Data.Count > 0)
                    {
                        Console.WriteLine(JsonSerializer.Serialize(exception.InnerException.Data, new JsonSerializerOptions() { WriteIndented = true }));
                    }
                }
            }
        };
    }

    public string GetAgentColor(string name)
    {
        return name switch
        {
            PresenterName => "blue",
            AudienceName => "green",
            ComedianName => "yellow",
            _ => "white"
        };
    }

    public override string ToString()
    {
        return "Multi-Agent";
    }
}