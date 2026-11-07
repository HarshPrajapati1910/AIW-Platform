import operator
from typing import Annotated, Any, Dict, List, Optional
from typing_extensions import TypedDict
from app.domain.models import CodeFinding, DomainScore, DomainType, ReviewRequest, Severity, FinalReviewReport

def take_latest(current: str, update: str) -> str:
    """Reducer that picks the latest step update when parallel nodes write concurrently."""
    return update

class ReviewGraphState(TypedDict):
    """
    Central immutable graph state container.
    findings uses LangGraph's operator.add reducer to aggregate concurrent agent emissions.
    """
    document_id: str
    filename: str
    source_code: str
    detected_language: Optional[str]
    activate_routes: List[str]

    findings: Annotated[List[CodeFinding], operator.add]
    domain_scores: Annotated[List[DomainScore], operator.add]

    executive_summary: Optional[str]
    final_report: Optional[FinalReviewReport]

    current_step: Annotated[str, take_latest]
    step_duration_ms: Dict[str, float]
    errors: Annotated[List[str], operator.add]
