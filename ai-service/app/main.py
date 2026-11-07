from fastapi import FastAPI, status
from app.config import settings
from app.schemas import AnalysisRequest, AnalysisResponse, HealthResponse
from app.services.ollama_service import ollama_service
from app.core.llm_factory import LLMFactory
import uvicorn
from fastapi.middleware.cors import CORSMiddleware
from app.api.routes import router as review_router

app = FastAPI(
    title=settings.app_name,
    version="1.0.0",
    docs_url="/docs",
    redoc_url="/redoc"
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(review_router)
@app.get("/health", response_model=HealthResponse, tags=["System"])
async def health_check():
    health = await ollama_service.check_health()
    is_healthy = health["ollama_connected"] and health["model_available"]

    return HealthResponse(
        status="HEALTHY" if is_healthy else "DEGRADED",
        ollama_connected=health["ollama_connected"],
        model_available=health["model_available"]
    )


@app.post(
    "/v1/analyze",
    response_model=AnalysisResponse,
    status_code=status.HTTP_200_OK,
    tags=["Analysis"]
)
async def analyze_document(request: AnalysisRequest):
    return await ollama_service.analyze_document(request)