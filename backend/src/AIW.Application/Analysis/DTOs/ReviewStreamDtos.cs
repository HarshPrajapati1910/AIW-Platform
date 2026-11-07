using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AIW.Application.Analysis.DTOs;

public record ReviewStreamEvent
(
    [property: JsonPropertyName("node")] string Node,
    [property: JsonPropertyName("current_step")] string CurrentStep,
    [property: JsonPropertyName("detected_language")] string? DetectedLanguage,
    [property: JsonPropertyName("findings_delta_count")] int FindingsDeltaCount,
    [property: JsonPropertyName("is_final")] bool IsFinal,
    [property: JsonPropertyName("report")] FinalReportPayload? Report
);

public record FinalReportPayload(
    [property: JsonPropertyName("document_id")] string DocumentId,
    [property: JsonPropertyName("overall_score")] int OverallScore,
    [property: JsonPropertyName("executive_summary")] string ExecutiveSummary,
    [property: JsonPropertyName("domain_scores")] List<DomainScoreDto> DomainScores,
    [property: JsonPropertyName("findings")] List<CodeFindingDto> Findings
);

public record DomainScoreDto(
    [property: JsonPropertyName("domain")] string Domain,
    [property: JsonPropertyName("score")] int Score,
    [property: JsonPropertyName("reasoning")] string Reasoning
);

public record CodeFindingDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("domain")] string Domain,
    [property: JsonPropertyName("severity")] string Severity,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("problematic_code")] string ProblematicCode,
    [property: JsonPropertyName("recommended_patch")] string RecommendedPatch
);