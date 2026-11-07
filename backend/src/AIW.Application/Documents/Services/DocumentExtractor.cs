using System.Text;
using AIW.Application.Common.Interfaces;
using UglyToad.PdfPig;

namespace AIW.Application.Documents.Services;

public class DocumentExtractor : IDocumentExtractor
{
    public async Task<string> ExtractTextAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            ".txt" => await ExtractFromTxtAsync(fileStream, cancellationToken),
            ".pdf" => ExtractFromPdf(fileStream),
            _ => throw new NotSupportedException($"File format '{extension}' is not supported. Upload .txt or .pdf.")
        };
    }

    private static async Task<string> ExtractFromTxtAsync(Stream fileStream, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(fileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        fileStream.Position = 0;
        return await reader.ReadToEndAsync(cancellationToken);
    }

    private static string ExtractFromPdf(Stream fileStream)
    {
        fileStream.Position = 0;
        var builder = new StringBuilder();

        using var pdfDocument = PdfDocument.Open(fileStream);
        foreach (var page in pdfDocument.GetPages())
        {
            builder.AppendLine(page.Text);
        }

        return builder.ToString();
    }
}