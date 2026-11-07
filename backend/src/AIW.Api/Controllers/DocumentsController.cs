using AIW.Application.Documents.DTOs;
using AIW.Application.Documents.Services;
using Microsoft.AspNetCore.Mvc;
using AIW.Application.Common.Interfaces;
using AIW.Application.Analysis.DTOs;
using AIW.Application.Analysis.Services;
using Microsoft.AspNetCore.Authorization;

namespace AIW.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly IDocumentExtractor _documentExtractor;
    private readonly IAiServiceClient _aiServiceClient;
    public DocumentsController(IDocumentService documentService, IDocumentExtractor documentExtractor, IAiServiceClient aiServiceClient)
    {
        _documentService = documentService;
        _documentExtractor = documentExtractor;
        _aiServiceClient = aiServiceClient;
    }

    /// <summary>
    /// Retrieves a list of all documents sorted by latest update.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DocumentListItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var documents = await _documentService.GetAllAsync(cancellationToken);
        return Ok(documents);
    }

    /// <summary>
    /// Retrieves a single document by its ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var document = await _documentService.GetByIdAsync(id, cancellationToken);
        if (document is null)
        {
            return NotFound(new { message = $"Document with ID '{id}' was not found." });
        }

        return Ok(document);
    }

    /// <summary>
    /// Creates a new document.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateDocumentRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _documentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates an existing document.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateDocumentRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _documentService.UpdateAsync(id, request, cancellationToken);
        if (updated is null)
        {
            return NotFound(new { message = $"Document with ID '{id}' was not found." });
        }

        return Ok(updated);
    }

    /// <summary>
    /// Deletes a document by ID.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var success = await _documentService.DeleteAsync(id, cancellationToken);
        if (!success)
        {
            return NotFound(new { message = $"Document with ID '{id}' was not found." });
        }

        return NoContent();
    }

    /// <summary>
    /// Uploads a document file (.pdf, .txt), extracts its content, and saves it.
    /// </summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "No file was uploaded." });
        }

        const long maxFileSize = 10 * 1024 * 1024; // 10MB limit
        if (file.Length > maxFileSize)
        {
            return BadRequest(new { message = "File size exceeds the 10MB limit." });
        }

        var allowedExtensions = new[] { ".pdf", ".txt" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new { message = "Only .pdf and .txt files are allowed." });
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);

        var extractedText = await _documentExtractor.ExtractTextAsync(memoryStream, file.FileName, cancellationToken);
        if (string.IsNullOrWhiteSpace(extractedText))
        {
            return BadRequest(new 
            { 
                message = "No extractable text found. The PDF may be scanned or image-only. AIW requires selectable text." 
            });
        }
        var title = Path.GetFileNameWithoutExtension(file.FileName);
        var request = new CreateDocumentRequest
        {
            Title = string.IsNullOrWhiteSpace(title) ? "Untitled Document" : title,
            Content = extractedText
        };

        var created = await _documentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Triggers AI analysis for a specified document.
    /// </summary>
    [HttpPost("{id:guid}/analysis")]
    [ProducesResponseType(typeof(DocumentAnalysisResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Analyze([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var document = await _documentService.GetByIdAsync(id, cancellationToken);
        if (document is null)
        {
            return NotFound(new { message = $"Document with ID '{id}' was not found." });
        }

        if (string.IsNullOrWhiteSpace(document.Content))
        {
            return BadRequest(new { message = "Cannot analyze a document with empty content." });
        }

        try
        {
            var analysis = await _aiServiceClient.AnalyzeDocumentAsync(document.Id, document.Content, cancellationToken);
            return Ok(analysis);
        }
        catch (TimeoutException ex)
        {
            return StatusCode(StatusCodes.Status504GatewayTimeout, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
    }   
}