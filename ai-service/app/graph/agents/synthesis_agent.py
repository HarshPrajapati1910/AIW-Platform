from typing import Any, Dict, List
from app.graph.agents.base_agent import BaseAgent
from app.domain.models import CodeFinding, DomainScore, DomainType, FinalReviewReport


class SynthesisScoringAgent(BaseAgent):
    def __init__(self):
        super().__init__(agent_name="SynthesisScoringAgent")

    async def run(self, state: Dict[str, Any]) -> Dict[str, Any]:
        system_prompt = (
            "You are an Engineering Director synthesising an audit report. "
            "Evaluate deduplicated findings and compute an overall health score (0-100) and domain scores. "
            "Output JSON:\n"
            "{\n"
            "  \"overall_score\": int,\n"
            "  \"executive_summary\": str,\n"
            "  \"domain_scores\": [\n"
            "    {\"domain\": \"Security\"|\"Performance\"|\"Architecture\", \"score\": int, \"reasoning\": str}\n"
            "  ]\n"
            "}"
        )
        
        current_findings: List[CodeFinding] = state.get("findings", [])
        finding_summaries = [
            f"[{f.domain.value} - {f.severity.value}] {f.title}: {f.description}" 
            for f in current_findings
        ]
        
        user_prompt = f"Total Findings: {len(current_findings)}\nItems:\n" + "\n".join(finding_summaries)
        payload = await self._invoke_structured_prompt(system_prompt, user_prompt)
        
        scores: List[DomainScore] = []
        for s in payload.get("domain_scores", []):
            try:
                scores.append(DomainScore(
                    domain=DomainType(s.get("domain")),
                    score=s.get("score", 70),
                    reasoning=s.get("reasoning", "")
                ))
            except Exception:
                continue

        overall = payload.get("overall_score", 75)
        summary = payload.get("executive_summary", "Review completed successfully.")

        final_report = FinalReviewReport(
            document_id=state.get("document_id", ""),
            overall_score=overall,
            executive_summary=summary,
            domain_scores=scores,
            findings=current_findings
        )

        return {
            "executive_summary": summary,
            "domain_scores": scores,
            "final_report": final_report,
            "current_step": "synthesis_completed"
        }