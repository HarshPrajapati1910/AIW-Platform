using System.ComponentModel.DataAnnotations;

namespace AIW.Application.Documents.DTOs;

public class CreateDocumentRequest
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters.")]
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;
}