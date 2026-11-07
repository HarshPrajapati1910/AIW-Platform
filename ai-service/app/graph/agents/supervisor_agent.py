from typing import Any, Dict
from app.graph.agents.base_agent import BaseAgent


class SupervisorAgent(BaseAgent):
    def __init__(self):
        super().__init__(agent_name="SupervisorAgent")

    async def run(self, state: Dict[str, Any]) -> Dict[str, Any]:
        system_prompt = (
            "You are an enterprise code supervisor. Determine programming language, "
            "architecture framework, and which specialized audit checks should run. "
            "Respond in JSON: {\"detected_language\": string, \"active_routes\": [\"security\", \"performance\", \"architecture\"]}"
        )
        user_prompt = f"File: {state.get('filename')}\nSource Code Sample:\n{state.get('source_code')[:1500]}"
        
        result = await self._invoke_structured_prompt(system_prompt, user_prompt)
        
        return {
            "detected_language": result.get("detected_language", "Unknown"),
            "active_routes": result.get("active_routes", ["security", "performance", "architecture"]),
            "current_step": "supervisor_completed"
        }