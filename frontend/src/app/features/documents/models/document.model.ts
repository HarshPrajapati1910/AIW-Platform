export interface DocumentListItem {
  id: string;
  title: string;
  preview: string;
  updatedAt: string;
}

export interface Document {
  id: string;
  title: string;
  content: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateDocumentRequest {
  title: string;
  content: string;
}

export interface UpdateDocumentRequest {
  title: string;
  content: string;
}

export interface CandidateTask {
  title: string;
  context: string;
}

export interface CandidateDecision {
  decision: string;
  context: string;
}

export interface CandidateIssue {
  issue: string;
  severity: 'High' | 'Medium' | 'Low' | string;
}

export interface DocumentAnalysis {
  documentId: string;
  modelUsed: string;
  summary: string;
  keyPoints: string[];
  candidateTasks: CandidateTask[];
  candidateDecisions: CandidateDecision[];
  candidateIssues: CandidateIssue[];
}
