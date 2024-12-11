// Inspired by https://github.com/microsoft/semantic-kernel/tree/main/dotnet/samples/Demos/VectorStoreRAG
// Find more pdf files here https://github.com/Azure-Samples/azure-search-sample-data/tree/main
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using SemanticKernelRag;

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0010

Console.ForegroundColor = ConsoleColor.DarkGreen;

Console.WriteLine("* Configure the kernel...");

var handler = new HttpClientHandler();
handler.CheckCertificateRevocationList = false; // We need to do this because of ZScaler
var client = new HttpClient(handler);

var openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new Exception("Create an environment variable named 'OPENAI_API_KEY' with your OpenAI API key");

var kernelBuilder = Kernel.CreateBuilder();

Console.WriteLine(" - Add chat completion");
kernelBuilder.AddOpenAIChatCompletion(
    modelId: "gpt-4o",
    apiKey: openAiApiKey,
    httpClient: client);

Console.WriteLine(" - Add embedding generator");
kernelBuilder.AddOpenAITextEmbeddingGeneration(
    modelId: "text-embedding-3-large",
    apiKey: openAiApiKey,
    httpClient: client);

Console.WriteLine(" - Add chat completion");
// TODO: Could also use the Qdrant connector to connect to a Qdrant vector DB instead
kernelBuilder.AddInMemoryVectorStoreRecordCollection<string, TextSnippet<string>>(
    "pdfcontent");

Console.WriteLine(" - Add vector store text search");
// Add a text search implementation that uses the registered vector store record collection for search.
kernelBuilder.AddVectorStoreTextSearch<TextSnippet<string>>(
    new TextSearchStringMapper((result) => (result as TextSnippet<string>)!.Text!),
    new TextSearchResultMapper((result) =>
    {
        // Create a mapping from the Vector Store data type to the data type returned by the Text Search.
        // This text search will ultimately be used in a plugin and this TextSearchResult will be returned to the prompt template
        // when the plugin is invoked from the prompt template.
        var castResult = result as TextSnippet<string>;
        return new TextSearchResult(value: castResult!.Text!) { Name = castResult.ReferenceDescription, Link = castResult.ReferenceLink };
    }));

Console.WriteLine("* Build kernel");
var kernel = kernelBuilder.Build();

Console.WriteLine("* Get services from kernel");
var vectorStoreRecordCollection = kernel.GetRequiredService<IVectorStoreRecordCollection<string, TextSnippet<string>>>();
var textEmbeddingGenerationService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
var vectorStoreTextSearch = kernel.GetRequiredService<VectorStoreTextSearch<TextSnippet<string>>>();

Console.WriteLine("* Create data loader");
UniqueKeyGenerator<Guid> guidKeyGenerator = new UniqueKeyGenerator<Guid>(() => Guid.NewGuid());
UniqueKeyGenerator<string> stringKeyGenerator = new UniqueKeyGenerator<string>(() => Guid.NewGuid().ToString());
IDataLoader dataLoader = new DataLoader<string>(stringKeyGenerator, vectorStoreRecordCollection, textEmbeddingGenerationService, chatCompletionService);


Console.WriteLine("* Load data file batch 1");
var filePathsBatch1 = new string[]
{   // TODO: Make this a configuration
    "Documents/employee_handbook.pdf",
    "Documents/benefit_options.pdf",
    "Documents/PerksPlus.pdf"
};
await LoadPdfAsync(filePathsBatch1);

Console.WriteLine("* Load data file batch 2");
var filePathsBatch2 = new string[]
{ 
    "Documents/role_library.pdf"
};
await LoadPdfAsync(filePathsBatch2);

// Add a search plugin to the kernel which we will use in the template below
// to do a vector search for related information to the user query.
//TODO: Can this be done in the kernel builder?
kernel.Plugins.Add(vectorStoreTextSearch.CreateWithGetTextSearchResults("SearchPlugin"));

while (true)
{
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.Write("> ");
    var question = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(question))
    {
        break;
    }
    
    var response = kernel.InvokePromptStreamingAsync(
        promptTemplate: """
                        Please use this information to answer the question:
                        {{#with (SearchPlugin-GetTextSearchResults question)}}  
                          {{#each this}}  
                            Name: {{Name}}
                            Value: {{Value}}
                            Link: {{Link}}
                            -----------------
                          {{/each}}
                        {{/with}}

                        Include citations to the relevant information where it is referenced in the response.

                        Question: {{question}}
                        """,
        arguments: new KernelArguments()
        {
            { "question", question },
        },
        templateFormat: "handlebars",
        promptTemplateFactory: new HandlebarsPromptTemplateFactory());
    
    Console.ForegroundColor = ConsoleColor.DarkMagenta;
    Console.Write("> ");
    
    await foreach (var message in response.ConfigureAwait(false))
    {
        Console.Write(message);
    }
    Console.WriteLine();
}

Console.ForegroundColor = ConsoleColor.DarkGreen;
Console.WriteLine("Exit");

return;


async Task LoadPdfAsync(string [] filePaths)
{
    var dataLoaderTask = LoadDataAsync(filePaths);
    while (!dataLoaderTask.IsCompleted)
    {
        await Task.Delay(1000).ConfigureAwait(false);
    }
    if (dataLoaderTask.IsFaulted)
    {
        Console.WriteLine("Failed to load data");
        return;
    }
}


/// <summary>
/// Load all configured PDFs into the vector store.
/// </summary>
/// <returns>An async task that completes when the loading is complete.</returns>
async Task LoadDataAsync(string[] pdfFilePaths, CancellationToken cancellationToken = default)     //TODO: Handle cancellation token better
{
  
    try
    {
        foreach (var pdfFilePath in pdfFilePaths)
        {
            Console.WriteLine($"  - Loading PDF into vector store: {pdfFilePath}");
            await dataLoader.LoadPdf(
                pdfFilePath,
                10,                 //TODO: Extract this to config
                1000,    //TODO: extract this to config
                cancellationToken).ConfigureAwait(false);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to load PDFs: {ex}");
        throw;
    }
}

