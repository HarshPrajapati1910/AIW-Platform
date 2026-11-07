export type FindingSeverity = 'Critical' | 'High' | 'Medium' | 'Low';
export type DomainType = 'Security' | 'Performance' | 'Architecture';

export interface CodeFinding {
  id: string;
  domain: DomainType;
  severity: FindingSeverity;
  title: string;
  description: string;
  problematic_code: string;
  recommended_patch: string;
}

export interface DomainScore {
  domain: DomainType;
  score: number;
  reasoning: string;
}

export interface FinalReviewReport {
  document_id: string;
  overall_score: number;
  executive_summary: string;
  domain_scores: DomainScore[];
  findings: CodeFinding[];
}

export interface StreamNodeEvent {
  node: string;
  current_step: string;
  detected_language?: string;
  findings_delta_count: number;
  is_final: boolean;
  report?: FinalReviewReport;
}

export interface NodeStepState {
  name: string;
  displayName: string;
  status: 'pending' | 'running' | 'completed' | 'failed';
  findingsCount: number;
}