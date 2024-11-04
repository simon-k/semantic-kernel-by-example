using System.ClientModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;
using OpenAI.Files;
using Spectre.Console;

namespace SemanticKernelByExample.Examples;

/// <summary>
/// Code interpreter example.
/// Based on https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/examples/example-assistant-code?pivots=programming-language-csharp
///
/// Try to ask it for
///  - "Average salary for employees" and get an average
///  - "Bar chart of employee salaries" and get a bar chart
///  - "Company values" and get a list of values but you'll see that it is not very good at the text...
/// </summary>
/// <param name="apiKey"></param>
public class CodeInterpreter(string apiKey) : Example
{
    [Experimental("SKEXP0110")]
    public override async Task ExecuteAsync(Kernel kernel)
    {
        var clientProvider = OpenAIClientProvider.ForOpenAI(new ApiKeyCredential(apiKey));
        
        Console.WriteLine("Uploading files...");
        var fileClient = clientProvider.Client.GetOpenAIFileClient();
        OpenAIFile employeesFile = await fileClient.UploadFileAsync("CompanyDocuments/employees.csv", FileUploadPurpose.Assistants);
        OpenAIFile employeeHandbookFile = await fileClient.UploadFileAsync("CompanyDocuments/employee_handbook.pdf", FileUploadPurpose.Assistants);
        
        Console.WriteLine("Defining agent...");
        var agent =
            await OpenAIAssistantAgent.CreateAsync(
                clientProvider,
                new OpenAIAssistantDefinition("gpt-4o-mini")
                {
                    Name = "SampleAssistantAgent",
                    Instructions =
                        """
                        Analyze the available data to provide an answer to the user's question.
                        Always format response using markdown.
                        Always sort lists in ascending order.
                        """,
                    EnableCodeInterpreter = true,
                    CodeInterpreterFileIds = [employeesFile.Id, employeeHandbookFile.Id],
                },
                new Kernel());
        
        Console.WriteLine("Creating thread...");
        string threadId = await agent.CreateThreadAsync();
        
        Console.WriteLine("Ready!");

        try
        {
            bool isComplete = false;
            List<string> fileIds = [];
            do
            {
                Console.WriteLine();
                Console.Write("> ");
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }
                if (input.Trim().Equals("EXIT", StringComparison.OrdinalIgnoreCase))
                {
                    isComplete = true;  // IS the break enough to exit the loop?
                    break;
                }

                await agent.AddChatMessageAsync(threadId, new ChatMessageContent(AuthorRole.User, input));

                Console.WriteLine();
                
                bool isCode = false;
                await foreach (StreamingChatMessageContent response in agent.InvokeStreamingAsync(threadId))
                {
                    if (isCode != (response.Metadata?.ContainsKey(OpenAIAssistantAgent.CodeInterpreterMetadataKey) ?? false))
                    {
                        Console.WriteLine();
                        isCode = !isCode;
                    }

                    // Display response.
                    Console.Write($"{response.Content}");

                    // Capture file IDs for downloading.
                    fileIds.AddRange(response.Items.OfType<StreamingFileReferenceContent>().Select(item => item.FileId));
                }
                Console.WriteLine();

                // Download any files referenced in the response.
                await DownloadResponseImageAsync(fileClient, fileIds);
                fileIds.Clear();

            } while (!isComplete);
        }
        finally
        {
            Console.WriteLine();
            Console.WriteLine("Cleaning-up...");
            await Task.WhenAll(
            [
                agent.DeleteThreadAsync(threadId),
                agent.DeleteAsync(),
                fileClient.DeleteFileAsync(employeesFile.Id),
                fileClient.DeleteFileAsync(employeeHandbookFile.Id)
            ]);
        }
        
        /*
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[green]This is a [bold][italic]stateless chat example[/][/]. It will not remember the context of the conversation.[/]");
        while (true)
        {
            var question = AnsiConsole.Ask<string>("[purple]Q?[/]");
            
            var answer = await kernel.InvokePromptAsync(question);
            
            AnsiConsole.MarkupLine($"[orange1]A:[/] {answer}");
        }*/
    }
    
    private static async Task DownloadResponseImageAsync(OpenAIFileClient client, ICollection<string> fileIds)
    {
        if (fileIds.Count > 0)
        {
            Console.WriteLine();
            foreach (string fileId in fileIds)
            {
                await DownloadFileContentAsync(client, fileId, launchViewer: true);
            }
        }
    }

    private static async Task DownloadFileContentAsync(OpenAIFileClient client, string fileId, bool launchViewer = false)
    {
        OpenAIFile fileInfo = client.GetFile(fileId);
        if (fileInfo.Purpose == FilePurpose.AssistantsOutput)
        {
            string filePath =
                Path.Combine(
                    //Path.GetTempPath(),
                    Path.GetFileName(Path.ChangeExtension(fileInfo.Filename, ".png")));

            BinaryData content = await client.DownloadFileAsync(fileId);
            await using FileStream fileStream = new(filePath, FileMode.CreateNew);
            await content.ToStream().CopyToAsync(fileStream);
            Console.WriteLine($"File saved to: {filePath}.");

            if (launchViewer)
            {
                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/C start {filePath}"
                    });
            }
        }
    }
    
    public override string ToString()
    {
        return "Code Interpreter Example";
    }
}