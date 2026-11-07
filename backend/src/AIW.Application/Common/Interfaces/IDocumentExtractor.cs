namespace AIW.Application.Common.Interfaces;

public interface IDocumentExtractor
{
    Task<string> ExtractTextAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
}