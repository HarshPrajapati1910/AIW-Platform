import json
from typing import AsyncGenerator
from fastapi import APIRouter
from sse_starlette.sse import EventSourceResponse
from app.domain.models import ReviewRequest
from app.graph.workflow import review_graph

router = APIRouter()


async def stream_review_execution(request: ReviewRequest) -> AsyncGenerator[str, None]:
    initial_state = {
        "document_id": request.document_id,
        "filename": request.filename,
        "source_code": request.source_code,
        "detected_language": None,
        "active_routes": [],
        "findings": [],
        "domain_scores": [],
        "executive_summary": None,
        "final_report": None,
        "current_step": "initialized",
        "step_duration_ms": {},
        "errors": []
    }

    # Stream event steps over LangGraph astream
    async for output in review_graph.astream(initial_state):
        for node_name, node_update in output.items():
            event_payload = {
                "node": node_name,
                "current_step": node_update.get("current_step"),
                "detected_language": node_update.get("detected_language"),
                "findings_delta_count": len(node_update.get("findings", [])),
                "is_final": "final_report" in node_update,
                "report": node_update.get("final_report").model_dump() if node_update.get("final_report") else None
            }
            yield json.dumps(event_payload)


@router.post("/v1/review/stream")
async def execute_review_stream(request: ReviewRequest):
    return EventSourceResponse(
        stream_review_execution(request),
        media_type="text/event-stream",
        headers={"X-Accel-Buffering": "no"}  # Prevent reverse-proxy SSE buffering
    )