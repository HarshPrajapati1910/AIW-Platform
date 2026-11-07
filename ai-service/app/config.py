from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    app_name: str = "AIW AI Service"
    ollama_base_url: str = "http://localhost:11434"
    model_name: str = "qwen2.5:3b"
    temperature: float = 0.1
    request_timeout_seconds: float = 180.0
    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        env_prefix="AIW_",
    )

    # class Config:
    #     env_prefix = "AIW_"


settings = Settings()