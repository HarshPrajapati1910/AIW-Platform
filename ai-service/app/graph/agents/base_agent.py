import abc
import json
import logging
from typing import Any, Dict
from langchain_core.messages import SystemMessage, HumanMessage
from app.core.llm_factory import LLMFactory

logger = logging.getLogger(__name__)

class BaseAgent(abc.ABC):
    """
    Independent base agent enforcing single responsibility, schema validation,
    and resilience against output hallucinations.
    """
    def __init__(self, agent_name: str):
        self.agent_name = agent_name
        self.llm = LLMFactory.get_json_llm()

    @abc.abstractmethod
    async def run(self, state: Dict[str, Any]) -> Dict[str, Any]:
        """Executes agent boundary logic and returns partial state update."""
        pass

    async def _invoke_structured_prompt(self, system_prompt: str, user_prompt: str) -> Dict[str, Any]:
        try:
            messages = [
                SystemMessage(content=system_prompt),
                HumanMessage(content=user_prompt)
            ]
            response = await self.llm.ainvoke(messages)
            return json.loads(response.content)
        except json.JSONDecodeError as jde:
            logger.error(f"[{self.agent_name}] JSON Decode error: {jde}")
            return {"findings": [], "error": f"Invalid JSON payload: {str(jde)}"}
        except Exception as ex:
            logger.error(f"[{self.agent_name}] Invocation error: {ex}")
            return {"findings": [], "error": str(ex)}