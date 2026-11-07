// AIW.Domain/Entities/DocumentReviewReport.cs
using System;
using System.Collections.Generic;

namespace AIW.Domain.Entities;

public class DocumentReviewReport
{
    public Guid Id { get; private set; }
    public Guid DocumentId { get; private set; }
    public int OverallScore { get; private set; }
    public string ExecutiveSummary { get; private set; } = string.Empty;
    public int SecurityScore { get; private set; }
    public int PerformanceScore { get; private set; }
    public int ArchitectureScore { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public List<DocumentReviewFinding> Findings { get; private set; } = new();

    protected DocumentReviewReport() { } // EF Core

    public DocumentReviewReport(
        Guid documentId,
        int overallScore,
        string executiveSummary,
        int securityScore,
        int performanceScore,
        int architectureScore)
    {
        Id = Guid.NewGuid();
        DocumentId = documentId;
        OverallScore = overallScore;
        ExecutiveSummary = executiveSummary;
        SecurityScore = securityScore;
        PerformanceScore = performanceScore;
        ArchitectureScore = architectureScore;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void AddFinding(string domain, string severity, string title, string description, string problematicCode, string recommendedPatch)
    {
        Findings.Add(new DocumentReviewFinding(Id, domain, severity, title, description, problematicCode, recommendedPatch));
    }
}

public class DocumentReviewFinding
{
    public Guid Id { get; private set; }
    public Guid ReportId { get; private set; }
    public string Domain { get; private set; } = string.Empty;
    public string Severity { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string ProblematicCode { get; private set; } = string.Empty;
    public string RecommendedPatch { get; private set; } = string.Empty;

    protected DocumentReviewFinding() { } // EF Core

    public DocumentReviewFinding(
        Guid reportId,
        string domain,
        string severity,
        string title,
        string description,
        string problematicCode,
        string recommendedPatch)
    {
        Id = Guid.NewGuid();
        ReportId = reportId;
        Domain = domain;
        Severity = severity;
        Title = title;
        Description = description;
        ProblematicCode = problematicCode;
        RecommendedPatch = recommendedPatch;
    }
}