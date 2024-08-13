# SemanticKernelByExample
A demo console that demonstrates the usage of Semantic Kernel.

There is a small bonus project that demonstrates how to use the Semantic Kernel with Azure AI Search. See the README.md in the SemanticKernelWithAzureAiSearch folder.

## Pre-requisites
You need to have

* An OpenAI API key
* A Bing Connecter API Key, if you want ~~~~to run the Web Search example

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

## Examples
### Stateless Chat
The console will ask you for a question and then it will use the Semantic Kernel to find the answer.
It will not store any state between questions.

```
Q? My name is Simon
A: Nice to meet you, Simon! How can I assist you today?
Q? What is my name?
A: As an AI, I have no personal data about users unless it has been shared with me in the course of our conversation. I am designed to respect user privacy and confidentiality.
```

### Stateful Chat
The console will ask you for a question and then it will use the Semantic Kernel to find the answer.
It will use the chat history to keep a state of the conversation. It cannot derrive any personal information from the chat history.

```
Q? My name is Simon
A: Nice to meet you, Simon! How can I assist you today?
Q? What is my name?
A: Your name is Simon.
Q? I am from Denmark
A: That's great! Denmark is known for its beautiful landscapes and rich historical background. How can I assist you today?
Q? What is my favorite food?
A: I'm sorry, but as an artificial intelligence, I don't have access to personal data about individuals unless it has been shared with me in the course of our conversation. I'm designed to respect user privacy and confidentiality.
```

### Kernel Function Chat
Kernel functions can be used to derrive details based on some logic that you define. Here is an example with a function that 
defines what different countries favorite food is. It cannot ansqwer questions about something that is "current" or happened after the model was trained.

```
Q? My name is Simon and I am from Denmark
A: Nice to meet you, Simon from Denmark. I can find out what the favorite food is in Denmark. Would you like to know?
Q? What is my favorite food?
A: According to demographic statistics, the favorite food in Denmark is Frikadeller. However, I don't have personal information about your specific favorite food, Simon.
Q? How is the weather tomorrow?
A: I'm sorry, I currently don't have the ability to provide weather updates. Is there anything else you would like to know?
```

### Bing Web Search Chat
The console will ask you for a question and then it will use the Semantic Kernel to find the answer. A Bing Web Search will be used to find the if needed.

```
Q? I am from Denmark      
A: That's great! How can I assist you today?
Q? How is the weather tomorrow?
A: The weather in Denmark tomorrow is expected to be around 22 degrees Celsius with thundery showers and light winds. Please note that weather conditions can change rapidly, so it's a good idea to check the local weather service for the most accurate forecast.
Q? What is the menu in the canteen on tuesday?
A: I'm sorry, but I wasn't able to find the exact canteen menu for Tuesday in Denmark. The menus often vary by location and provider. I recommend checking the specific canteen's website or contacting them directly for the most accurate information.
```

### Website Content Chat / Custom Knowledge Base
It is possible to make Kernel Functions that look up data on a website or for example in json documents.

```
Q: Whats the menu on Tuesday?
A: The menu for Tuesday is Indian pakoras with bulgur salad with feta, squash and pimento with tzatziki.
Q: Who is the Chef?
A: The chef is Ursula.
Q: What are the opening hours on Sunday?
A: The canteen is closed on Sunday.
```

### Meta Prompt Chat
Instruct the AI on how to behave and respond. This one will be friendly, funny and only know about tacos.

```
Q? Hi. Give me a burger recipy
A: Oh, my funny friend, you've made a misstep! Like trying to use hot sauce instead of ketchup, you're asking a taco guru for a burger recipe. But don't worry, I can turn any meal into a taco fest. Let's make a cheeseburger taco!

Ingredients:
- 1 lb ground beef 
- 1 onion, chopped
- 1 packet taco seasoning
- sliced cheese 
....
```

## Other stuff
### Logging
You can add a console logger to the kernel builder if you want to see how it gets to the result that it does.

```csharp
kernelBuilder.Services.AddLogging(loggingBuilder => loggingBuilder.AddConsole().SetMinimumLevel(LogLevel.Trace));
```

This is very useful if exceptions occur. For example when the Bing Connector does not work because of an SSL issue caused by ZScaler or another proxy.

## References
* [How to use Semantic Kernel with Azure AI Search.](https://devblogs.microsoft.com/semantic-kernel/azure-openai-on-your-data-with-semantic-kernel/)
* [Semantic Kernel with enterprise data tutorial](https://github.com/Azure-Samples/semantic-kernel-rag-chat)
* Video: [Infusing your .NET Apps with AI: Practical Tools and Techniques~~~~](https://www.youtube.com/watch?v=jrNfKeGSuCg)
* Video: [Scott and Mark learn AI](https://youtu.be/KKWPSkYN3vw?si=Or5HS5YoWYlkXTO0)