import json
import httpx
from fastapi import HTTPException
from app.config import settings
from app.schemas import AnalysisRequest, AnalysisResponse


class OllamaService:
    def __init__(self):
        self.base_url = settings.ollama_base_url.rstrip("/")
        self.model = settings.model_name
        self.timeout = settings.request_timeout_seconds

    async def check_health(self) -> dict:
        """Verifies Ollama daemon connectivity and checks if the configured model is installed."""
        try:
            async with httpx.AsyncClient(timeout=3.0) as client:
                res = await client.get(f"{self.base_url}/api/tags")
                if res.status_code != 200:
                    return {"ollama_connected": False, "model_available": False}

                data = res.json()
                models = [m.get("name") for m in data.get("models", [])]
                model_exists = any(self.model in m for m in models)
                return {"ollama_connected": True, "model_available": model_exists}
        except Exception:
            return {"ollama_connected": False, "model_available": False}

    async def analyze_document(self, req: AnalysisRequest) -> AnalysisResponse:
        prompt = f"""
You are an expert AI document analyst.
Analyze the following document content and extract structured insights.

CRITICAL INSTRUCTIONS:
- You must respond with valid JSON matching the exact schema below.
- Do NOT output preamble, markdown formatting, or triple backticks.
- Be grounded strictly in the provided text.

Required JSON format:
{{
  "summary": "2-3 concise summary sentences",
  "key_points": ["point 1", "point 2"],
  "candidate_tasks": [{{"title": "task title", "context": "source context"}}],
  "candidate_decisions": [{{"decision": "decision text", "context": "context"}}],
  "candidate_issues": [{{"issue": "risk or issue", "severity": "Low|Medium|High"}}]
}}

DOCUMENT CONTENT:
\"\"\"
{req.content}
\"\"\"
"""

        payload = {
            "model": self.model,
            "prompt": prompt,
            "stream": False,
            "format": "json",
            "options": {
                "temperature": 0.2
            }
        }

        try:
            async with httpx.AsyncClient(timeout=self.timeout) as client:
                response = await client.post(f"{self.base_url}/api/generate", json=payload)

            if response.status_code != 200:
                raise HTTPException(
                    status_code=502,
                    detail=f"Ollama returned error status {response.status_code}: {response.text}"
                )

            data = response.json()
            raw_response = data.get("response", "{}")
            parsed_json = json.loads(raw_response)

            return AnalysisResponse(
                document_id=req.document_id,
                model_used=self.model,
                summary=parsed_json.get("summary", ""),
                key_points=parsed_json.get("key_points", []),
                candidate_tasks=parsed_json.get("candidate_tasks", []),
                candidate_decisions=parsed_json.get("candidate_decisions", []),
                candidate_issues=parsed_json.get("candidate_issues", [])
            )

        except httpx.ConnectError:
            raise HTTPException(
                status_code=503,
                detail=f"Cannot connect to Ollama at {self.base_url}. Make sure 'ollama serve' is running."
            )
        except httpx.TimeoutException:
            raise HTTPException(
                status_code=504,
                detail=f"AI model inference timed out after {self.timeout} seconds."
            )
        except json.JSONDecodeError as exc:
            raise HTTPException(
                status_code=500,
                detail=f"LLM produced malformed non-JSON output: {str(exc)}"
            )


ollama_service = OllamaService()