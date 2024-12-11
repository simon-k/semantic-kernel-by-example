// Copyright (c) Microsoft. All rights reserved.
// From https://github.com/microsoft/semantic-kernel/tree/main/dotnet/samples/Demos/VectorStoreRAG

using Microsoft.Extensions.VectorData;

namespace SemanticKernelRag;

/// <summary>
/// Data model for storing a section of text with an embedding and an optional reference link.
/// </summary>
/// <typeparam name="TKey">The type of the data model key.</typeparam>
internal sealed class TextSnippet<TKey>
{
    [VectorStoreRecordKey]
    public required TKey Key { get; set; }

    [VectorStoreRecordData]
    public string? Text { get; set; }

    [VectorStoreRecordData]
    public string? ReferenceDescription { get; set; }

    [VectorStoreRecordData]
    public string? ReferenceLink { get; set; }

    [VectorStoreRecordVector(1536)]
    public ReadOnlyMemory<float> TextEmbedding { get; set; }
}
