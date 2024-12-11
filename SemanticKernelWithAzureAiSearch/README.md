# Semantic Kernel With Azure AI Search Example
This is an example of a Semantic Kernel that uses Azure AI Search to find answers to questions.

It is based on this example:https://devblogs.microsoft.com/semantic-kernel/azure-openai-on-your-data-with-semantic-kernel/

## How to run
To get started make sure that you have am Azure Subscription with

1. An Azure OpenAI service
2. An Azure AI Search service
3. An Azure Blob Storage account

Populate the AI Search index with the data that you are sure the LLM model does know about. Fx this demo data https://github.com/Azure-Samples/azure-search-sample-data/blob/main/health-plan/employee_handbook.pdf

Make sure you have the correct values in these environment variables.

```
AZURE_OPENAI_ENDPOINT=REPLACE_WITH_YOUR_AOAI_ENDPOINT_VALUE_HERE
AZURE_OPENAI_API_KEY=REPLACE_WITH_YOUR_AOAI_KEY_VALUE_HERE
AZURE_OPENAI_DEPLOYMENT_NAME=REPLACE_WITH_YOUR_AOAI_DEPLOYMENT_VALUE_HERE
AZURE_AI_SEARCH_ENDPOINT=REPLACE_WITH_YOUR_AZURE_SEARCH_ENDPOINT_VALUE_HERE
AZURE_AI_SEARCH_API_KEY=REPLACE_WITH_YOUR_AZURE_SEARCH_API_KEY_VALUE_HERE
AZURE_AI_SEARCH_INDEX=REPLACE_WITH_YOUR_INDEX_NAME_HERE
```

and then run the console app

```
dotnet run
```

Example output where the Contorso Employee Handbook is loaded in Azure Search AI:

```
Ask a question before the Azure AI Search extension is loaded
Q: What are the contorso company values?
A: As an AI language model, I do not have access to specific information about company values of Contorso. Could you please provide me with more context or information, or rephrase your question?
Ask a question after the Azure AI Search extension is loaded
Q: What are the contorso company values?
A: The core values of Contoso Electronics are quality, integrity, innovation, teamwork, respect, excellence, accountability, and community [doc1].
```

## How to create the prerequisites

### Create an Azure OpenAI service
1. Go to the portal and create an Azure OpenAI service.
2. When it is created, go to the service and its model deployments to deploy fx the gpt4o-mini model and the text-embedding-ada-002 model.

### Create an Azure Blob Storage account
1. Go to the portal and create a standard Azure Blob Storage account
2. Create a container and upload a document that you want to search in Azure AI Search

### Create an Azure AI Search service
1. Go to the portal and create an Azure AI Search service.
2. When the service is created, go to the service and click "Import and vectorize data"


