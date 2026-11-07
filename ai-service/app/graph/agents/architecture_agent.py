import uuid
from typing import Any, Dict, List
from app.graph.agents.base_agent import BaseAgent
from app.domain.models import CodeFinding, DomainType, Severity


class ArchitectureSpecialistAgent(BaseAgent):
    def __init__(self):
        super().__init__(agent_name="ArchitectureSpecialistAgent")

    async def run(self, state: Dict[str, Any]) -> Dict[str, Any]:
        system_prompt = (
            "You are a Software Architect reviewing for Clean Architecture and SOLID standards. "
            "Check for: domain pollution, persistence concerns in domain entities, missing CancellationTokens, "
            "blocking async calls (.Result, .Wait()), and improper coupling. "
            "Output JSON with this schema:\n"
            "{\"findings\": [{\"title\": str, \"severity\": \"Critical\"|\"High\"|\"Medium\"|\"Low\", "
            "\"description\": str, \"problematic_code\": str, \"recommended_patch\": str}]}"
        )
        user_prompt = f"Language: {state.get('detected_language')}\nCode:\n{state.get('source_code')}"
        
        payload = await self._invoke_structured_prompt(system_prompt, user_prompt)
        raw_findings = payload.get("findings", [])
        
        parsed_findings: List[CodeFinding] = []
        for item in raw_findings:
            try:
                parsed_findings.append(
                    CodeFinding(
                        id=str(uuid.uuid4()),
                        domain=DomainType.ARCHITECTURE,
                        severity=Severity(item.get("severity", Severity.MEDIUM)),
                        title=item.get("title", "Architectural Deviation"),
                        description=item.get("description", ""),
                        problematic_code=item.get("problematic_code", ""),
                        recommended_patch=item.get("recommended_patch", "")
                    )
                )
            except Exception:
                continue

        return {
            "findings": parsed_findings,
            "current_step": "architecture_completed"
        }