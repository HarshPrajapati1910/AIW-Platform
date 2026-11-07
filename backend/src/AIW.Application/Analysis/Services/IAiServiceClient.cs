using AIW.Application.Analysis.DTOs;

namespace AIW.Application.Analysis.Services;

public interface IAiServiceClient
{
    Task<DocumentAnalysisResponse> AnalyzeDocumentAsync(Guid documentId, string content, CancellationToken cancellationToken = default);
}