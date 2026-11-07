using AIW.Application.Documents.DTOs;

namespace AIW.Application.Documents.Services;

public interface IDocumentService
{
    Task<IEnumerable<DocumentListItemResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DocumentResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DocumentResponse> CreateAsync(CreateDocumentRequest request, CancellationToken cancellationToken = default);
    Task<DocumentResponse?> UpdateAsync(Guid id, UpdateDocumentRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}