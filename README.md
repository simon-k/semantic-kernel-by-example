# SemanticKernelByExample
A demo console that demonstrates the usage of Semantic Kernel

## Pre-requisites
You need to have

* An OpenAI API key
* A Bing Connecter API Key

## How to run

Create environment variables for the API keys

```
export OPENAI_API_KEY=<your key>
export BING_CONNECTOR_API_KEY=<your key>
```

Run the console application

```
dotnet run
```

## Things not covered
### Logging
You can add a console logger to the kernel builder if you want to see how it gets to the result that it does

```csharp
kernelBuilder.Services.AddLogging(loggingBuilder => loggingBuilder.AddConsole().SetMinimumLevel(LogLevel.Warning));
```

### Chat Definition
You can define how the chat bot should behave and what kind of questions it should answer. The simplest way to do this
is to define it when creating the chat history.

```csharp
var chatHistory = new ChatHistory("You are a extremely friendly assistant that only knows about tacos recipes. Every time you answer a question you tell a taco joke");
```