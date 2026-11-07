namespace AIW.Application.Analysis.DTOs;

public class CandidateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
}

public class CandidateDecisionDto
{
    public string Decision { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
}

public class CandidateIssueDto
{
    public string Issue { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
}

public class DocumentAnalysisResponse
{
    public Guid DocumentId { get; set; }
    public string ModelUsed { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<string> KeyPoints { get; set; } = [];
    public List<CandidateTaskDto> CandidateTasks { get; set; } = [];
    public List<CandidateDecisionDto> CandidateDecisions { get; set; } = [];
    public List<CandidateIssueDto> CandidateIssues { get; set; } = [];
}