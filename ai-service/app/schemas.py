from typing import List, Optional
from uuid import UUID
from pydantic import BaseModel, Field


# Internal schemas for structured extraction
class CandidateTask(BaseModel):
    title: str = Field(description="Short, actionable task title")
    context: str = Field(description="Brief explanation of why this task is needed from the text")


class CandidateDecision(BaseModel):
    decision: str = Field(description="Clear statement of the decision made")
    context: str = Field(description="Context or rationale mentioned in the document")


class CandidateIssue(BaseModel):
    issue: str = Field(description="Description of the issue, blocker, or risk")
    severity: str = Field(description="Low, Medium, or High based on document severity")


# Main API schemas
class AnalysisRequest(BaseModel):
    document_id: UUID
    content: str = Field(min_length=10, description="Raw extracted document text")
    operation: str = Field(default="ANALYZE_DOCUMENT")


class AnalysisResponse(BaseModel):
    document_id: UUID
    model_used: str
    summary: str
    key_points: List[str]
    candidate_tasks: List[CandidateTask]
    candidate_decisions: List[CandidateDecision]
    candidate_issues: List[CandidateIssue]


class HealthResponse(BaseModel):
    status: str
    ollama_connected: bool
    model_available: bool