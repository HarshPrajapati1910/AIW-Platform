using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading;
using AIW.Application.Common.Interfaces;

namespace AIW.Application.Documents.Services;

public class AgentReviewClient : IAgentReviewClient
{
    private readonly HttpClient _httpClient;

    public AgentReviewClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async IAsyncEnumerable<string> StreamReviewAsync(
        Guid documentId,
        string filename,
        string sourceCode,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var requestPayload = new
        {
            document_id = documentId.ToString(),
            filename = filename,
            source_code = sourceCode
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/review/stream")
        {
            Content = JsonContent.Create(requestPayload)
        };

        // HttpCompletionOption.ResponseHeadersRead ensures headers return immediately without waiting for content body
        using var response = await _httpClient.SendAsync(
            request, 
            HttpCompletionOption.ResponseHeadersRead, 
            cancellationToken);

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is not null)
            {
                yield return line;
            }
        }
    }
}