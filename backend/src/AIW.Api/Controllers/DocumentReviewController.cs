using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AIW.Application.Common.Interfaces;
using AIW.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AIW.Application.Analysis.DTOs;

namespace AIW.Api.Controllers;

[ApiController]
[Route("api/documents")]public class DocumentReviewController: ControllerBase
{
    private readonly IAgentReviewClient _reviewClient;
    private readonly IApplicationDbContext _context;

    public DocumentReviewController(IAgentReviewClient reviewClient, IApplicationDbContext context)
    {
        _reviewClient = reviewClient;
        _context = context;
    }

    [HttpGet("{id:guid}/review/stream")]
    public async Task streamReview(Guid id, CancellationToken cancellationToken)
    {
        var document = await _context.Documents.FindAsync(new object[] { id }, cancellationToken);
        if (document is null)
        {
            Response.StatusCode = 404;
            return;
        }
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("X-Accel-Buffering", "no");

        FinalReportPayload? terminalReport = null;

        await foreach (var line in _reviewClient.StreamReviewAsync(
            document.Id, 
            document.Title ?? "Document.cs", 
            document.Content ?? string.Empty, 
            cancellationToken))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                await Response.WriteAsync("\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
                continue;
            }

            // Inspect data line for final report persistence
            if (line.StartsWith("data: "))
            {
                var jsonContent = line["data: ".Length..];
                try
                {
                    var streamEvent = JsonSerializer.Deserialize<ReviewStreamEvent>(jsonContent);
                    if (streamEvent?.IsFinal == true && streamEvent.Report is not null)
                    {
                        terminalReport = streamEvent.Report;
                    }
                }
                catch (JsonException) { /* Non-breaking parse telemetry */ }
            }

            await Response.WriteAsync($"{line}\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }

        if (terminalReport is not null)
        {
            var secScore = terminalReport.DomainScores.FirstOrDefault(s => s.Domain == "Security")?.Score ?? 0;
            var perfScore = terminalReport.DomainScores.FirstOrDefault(s => s.Domain == "Performance")?.Score ?? 0;
            var archScore = terminalReport.DomainScores.FirstOrDefault(s => s.Domain == "Architecture")?.Score ?? 0;

            var entity = new DocumentReviewReport(
                document.Id,
                terminalReport.OverallScore,
                terminalReport.ExecutiveSummary,
                secScore,
                perfScore,
                archScore
            );

            foreach (var f in terminalReport.Findings)
            {
                entity.AddFinding(f.Domain, f.Severity, f.Title, f.Description, f.ProblematicCode, f.RecommendedPatch);
            }

            _context.DocumentReviewReports.Add(entity);
            await _context.SaveChangesAsync(CancellationToken.None); // Complete persistence even if client disconnects at end
        }
    }
}