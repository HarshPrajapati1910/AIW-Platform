from langchain_ollama import ChatOllama
from app.config import settings

class LLMFactory:
    @staticmethod
    def get_json_llm() -> ChatOllama:
        """Returns deterministic, JSON-enforced Ollama instance."""
        return ChatOllama(
            base_url = settings.ollama_base_url,
            model = settings.model_name,
            temparature = settings.temperature,
            format="json",
            timeout=settings.request_timeout_seconds
        )