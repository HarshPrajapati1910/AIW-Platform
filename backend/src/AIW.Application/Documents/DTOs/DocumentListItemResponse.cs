namespace AIW.Application.Documents.DTOs;

public class DocumentListItemResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Preview { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
}