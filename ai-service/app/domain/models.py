from enum import Enum
from typing import List, Optional
from pydantic import BaseModel, Field

class Severity(str, Enum):
    CRITICAL = "Critical"
    HIGH = "High"
    MEDIUM = "Medium"
    LOW = "Low"

class DomainType(str, Enum):
    SECURITY = "Security"
    PERFORMANCE = "Performance"
    ARCHITECTURE = "Architecture"

class CodeFinding(BaseModel):
    id: str = Field(..., description="Unique UUID for finding")
    domain: DomainType
    severity: Severity
    title: str
    description: str
    line_number_approx: Optional[int] = None
    problematic_code: str
    recommended_patch: str

class DomainScore(BaseModel):
    domain: DomainType
    score: int = Field(..., ge=0, le=100)
    reasoning: str

class ReviewRequest(BaseModel):
    document_id: str
    filename: str
    source_code: str

class FinalReviewReport(BaseModel):
    document_id: str
    overall_score: int = Field(..., ge=0, le=100)
    executive_summary: str
    domain_scores: List[DomainScore] = Field(default_factory=list)
    findings: List[CodeFinding] = Field(default_factory=list)