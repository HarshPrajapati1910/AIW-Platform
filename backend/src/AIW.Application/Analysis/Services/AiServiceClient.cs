using System.Net.Http.Json;
using System.Text.Json;
using AIW.Application.Analysis.DTOs;
using Microsoft.Extensions.Logging;

namespace AIW.Application.Analysis.Services;

public class AiServiceClient : IAiServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AiServiceClient> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public AiServiceClient(HttpClient httpClient, ILogger<AiServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<DocumentAnalysisResponse> AnalyzeDocumentAsync(Guid documentId, string content, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            document_id = documentId,
            content = content,
            operation = "ANALYZE_DOCUMENT"
        };

        _logger.LogInformation("Dispatching analysis request to AI service for Document {DocumentId}", documentId);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync("/v1/analyze", payload, JsonOptions, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to FastAPI AI service.");
            throw new InvalidOperationException("The AI analysis service is currently unreachable.", ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "AI service request timed out.");
            throw new TimeoutException("The AI model inference timed out.", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("AI service returned status code {StatusCode}: {ErrorBody}", response.StatusCode, errorBody);
            throw new HttpRequestException($"AI service error ({response.StatusCode}): {errorBody}");
        }

        var result = await response.Content.ReadFromJsonAsync<DocumentAnalysisResponse>(JsonOptions, cancellationToken);
        if (result is null)
        {
            throw new InvalidOperationException("Failed to deserialize response from AI service.");
        }

        _logger.LogInformation("Successfully received AI analysis for Document {DocumentId} using model {Model}", documentId, result.ModelUsed);
        return result;
    }
}