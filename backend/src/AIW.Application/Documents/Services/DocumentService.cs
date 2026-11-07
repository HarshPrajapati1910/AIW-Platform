using AIW.Application.Common.Interfaces;
using AIW.Application.Documents.DTOs;
using AIW.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AIW.Application.Documents.Services;

public class DocumentService : IDocumentService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(IApplicationDbContext context, ILogger<DocumentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<DocumentListItemResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .AsNoTracking()
            .OrderByDescending(d => d.UpdatedAt)
            .Select(d => new DocumentListItemResponse
            {
                Id = d.Id,
                Title = d.Title,
                Preview = d.Content.Length > 100 ? d.Content.Substring(0, 100) + "..." : d.Content,
                UpdatedAt = d.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<DocumentResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _context.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (document is null)
        {
            _logger.LogWarning("Document with ID {DocumentId} not found.", id);
            return null;
        }

        return MapToResponse(document);
    }

    public async Task<DocumentResponse> CreateAsync(CreateDocumentRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var document = new Document
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Content = request.Content,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Document {DocumentId} created successfully.", document.Id);
        return MapToResponse(document);
    }

    public async Task<DocumentResponse?> UpdateAsync(Guid id, UpdateDocumentRequest request, CancellationToken cancellationToken = default)
    {
        var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (document is null)
        {
            _logger.LogWarning("Update failed. Document {DocumentId} not found.", id);
            return null;
        }

        document.Title = request.Title.Trim();
        document.Content = request.Content;
        document.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Document {DocumentId} updated successfully.", document.Id);
        return MapToResponse(document);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (document is null)
        {
            _logger.LogWarning("Delete failed. Document {DocumentId} not found.", id);
            return false;
        }

        _context.Documents.Remove(document);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Document {DocumentId} deleted successfully.", id);
        return true;
    }

    private static DocumentResponse MapToResponse(Document document) => new()
    {
        Id = document.Id,
        Title = document.Title,
        Content = document.Content,
        CreatedAt = document.CreatedAt,
        UpdatedAt = document.UpdatedAt
    };
}