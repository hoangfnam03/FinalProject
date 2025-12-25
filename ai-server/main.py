import os
import json
import uuid
import time
import traceback
import platform
from datetime import datetime
from pathlib import Path
from typing import Any, Dict, List, Optional

import torch
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field
from transformers import AutoModelForCausalLM, AutoTokenizer
from fastapi.responses import JSONResponse
from fastapi.middleware.cors import CORSMiddleware

# -----------------------------
# Config
# -----------------------------
MODEL_ID = os.getenv("MODEL_ID", "Qwen/Qwen2.5-0.5B-Instruct")
PORT = int(os.getenv("PORT", "8001"))

# FE của bạn đang chạy 127.0.0.1:5504 (Live Server)
CORS_ORIGINS = os.getenv(
    "CORS_ORIGINS",
    "http://127.0.0.1:5504,http://localhost:5504,http://127.0.0.1:5500,http://localhost:5500",
).split(",")

MAX_NEW_TOKENS = int(os.getenv("MAX_NEW_TOKENS", "512"))
TEMPERATURE = float(os.getenv("TEMPERATURE", "0.7"))
TOP_P = float(os.getenv("TOP_P", "0.9"))
REPETITION_PENALTY = float(os.getenv("REPETITION_PENALTY", "1.05"))

# Dev: bật trả stacktrace ra response khi lỗi (tắt trong production)
DEBUG_STACKTRACE = os.getenv("DEBUG_STACKTRACE", "1").lower() in ("1", "true", "yes", "y")

DATA_DIR = Path(os.getenv("DATA_DIR", "data"))
FEEDBACK_PATH = DATA_DIR / "feedback.jsonl"

# -----------------------------
# FastAPI
# -----------------------------
app = FastAPI(title="DevAsk AI Server", version="1.0")


app.add_middleware(
    CORSMiddleware,
    allow_origins=[
        "http://127.0.0.1:5504",
        "http://localhost:5504",
    ],
    allow_credentials=False,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=[o.strip() for o in CORS_ORIGINS if o.strip()],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# -----------------------------
# Models
# -----------------------------
tokenizer = None
model = None
model_ready = False
model_error = None

# Dùng lock để tránh generate đè nhau nếu có nhiều request cùng lúc
import asyncio
_generate_lock = asyncio.Lock()


class ChatRequest(BaseModel):
    org_id: str = Field(default="default")
    question_id: Optional[int] = None
    title: str = Field(default="")
    body: str = Field(default="")
    tags: List[str] = Field(default_factory=list)
    category: Optional[str] = None
    language: str = Field(default="vi")


class ChatResponse(BaseModel):
    answer: str
    disclaimer: str
    request_id: str


class FeedbackRequest(BaseModel):
    org_id: str = Field(default="default")
    question_id: Optional[int] = None
    question: Dict[str, Any] = Field(default_factory=dict)  # {title, body, tags, category}
    answer: str
    is_admin: bool = Field(default=False)
    meta: Dict[str, Any] = Field(default_factory=dict)


def _can_use_4bit() -> bool:
    """
    bitsandbytes 4-bit chỉ hoạt động khi:
    - Có CUDA khả dụng
    - Và gói bitsandbytes có CUDA binary tương ứng (Windows cần *.dll; Linux cần *.so)
    Trên Windows, pip bitsandbytes thường là CPU-only nên phải fallback fp16.
    """
    if not torch.cuda.is_available():
        return False
    try:
        import bitsandbytes as bnb  # noqa: F401
        bnb_dir = Path(bnb.__file__).resolve().parent
        sys_name = platform.system().lower()
        if "windows" in sys_name:
            return any(bnb_dir.glob("libbitsandbytes_cuda*.dll"))
        if "linux" in sys_name:
            return any(bnb_dir.glob("libbitsandbytes_cuda*.so"))
        # macOS không có CUDA; trả False an toàn
        return False
    except Exception:
        return False


def _load_model():
    global tokenizer, model, model_ready, model_error

    device = "cuda" if torch.cuda.is_available() else "cpu"
    print(f"[AI] Loading model: {MODEL_ID}")
    print(f"[AI] Device: {device}")

    tokenizer = AutoTokenizer.from_pretrained(MODEL_ID, trust_remote_code=True)
    # Một số model không có pad_token (hay gặp với chat models)
    if tokenizer.pad_token_id is None:
        tokenizer.pad_token = tokenizer.eos_token

    requested_4bit = os.getenv("USE_4BIT", "1").lower() in ("1", "true", "yes", "y")
    use_4bit = requested_4bit and _can_use_4bit()
    if requested_4bit and not use_4bit:
        print("[AI] ⚠️ Requested 4-bit but bitsandbytes CUDA binary is not available on this machine. Fallback fp16.")

    print(f"[AI] 4-bit enabled: {use_4bit}")

    if use_4bit:
        from transformers import BitsAndBytesConfig

        qconfig = BitsAndBytesConfig(
            load_in_4bit=True,
            bnb_4bit_quant_type="nf4",
            bnb_4bit_use_double_quant=True,
            bnb_4bit_compute_dtype=torch.float16,
        )

        model = AutoModelForCausalLM.from_pretrained(
            MODEL_ID,
            device_map="auto",
            trust_remote_code=True,
            quantization_config=qconfig,
        )
    else:
        # fp16 trên GPU (nhẹ và dễ), CPU thì fp32
        dtype = torch.float16 if device == "cuda" else torch.float32
        model = AutoModelForCausalLM.from_pretrained(
            MODEL_ID,
            device_map="auto" if device == "cuda" else None,
            trust_remote_code=True,
            torch_dtype=dtype,
        )
        # Nếu không dùng device_map, đưa model lên GPU thủ công
        if device == "cuda" and getattr(model, "hf_device_map", None) is None:
            model.to("cuda")

    model_ready = True
    model_error = None
    print("[AI] Model loaded.")


def _get_input_device() -> torch.device:
    # Best-effort: put input_ids on the same device as the input embedding
    if model is None:
        return torch.device("cuda" if torch.cuda.is_available() else "cpu")

    try:
        emb = model.get_input_embeddings()
        if emb is not None and hasattr(emb, "weight") and emb.weight is not None:
            return emb.weight.device
    except Exception:
        pass

    # If using accelerate device_map, prefer a real (non-cpu/disk/meta) device
    hf_map = getattr(model, "hf_device_map", None)
    if isinstance(hf_map, dict) and hf_map:
        for dev in hf_map.values():
            if isinstance(dev, str) and dev not in ("cpu", "disk", "meta"):
                return torch.device(dev)
        return torch.device("cpu")

    # Fallback
    try:
        return next(model.parameters()).device
    except StopIteration:
        return torch.device("cuda" if torch.cuda.is_available() else "cpu")



def _build_prompt(req: ChatRequest) -> List[Dict[str, str]]:
    # System prompt cố định để giảm trả lời linh tinh
    system = (
        "Bạn là trợ lý Q&A cho một cộng đồng hỏi đáp. "
        "Hãy trả lời ngắn gọn, đúng trọng tâm, có bước thực hiện nếu phù hợp. "
        "Nếu không chắc chắn, hãy nói rõ giả định và gợi ý cách kiểm chứng."
    )

    # Gộp title + body để model hiểu ngữ cảnh
    user_parts = []
    if req.title.strip():
        user_parts.append(f"Tiêu đề: {req.title.strip()}")
    if req.body.strip():
        user_parts.append(f"Nội dung: {req.body.strip()}")
    if req.tags:
        user_parts.append(f"Tags: {', '.join(req.tags)}")
    if req.category:
        user_parts.append(f"Danh mục: {req.category}")

    user = "\n".join(user_parts) if user_parts else "(Không có nội dung)"

    return [
        {"role": "system", "content": system},
        {"role": "user", "content": user},
    ]


def _generate_answer(req: ChatRequest) -> str:
    if tokenizer is None or model is None:
        raise RuntimeError("Model not loaded yet.")

    messages = _build_prompt(req)

    # ✅ Tạo prompt string từ chat template
    prompt = tokenizer.apply_chat_template(
        messages,
        tokenize=False,
        add_generation_prompt=True,
    )

    # ✅ Tokenize prompt -> BatchEncoding
    inputs = tokenizer(prompt, return_tensors="pt")

    # ✅ Đưa inputs lên đúng device (theo device của embedding)
    input_device = _get_input_device()
    try:
        inputs = inputs.to(input_device)
    except Exception:
        inputs = {k: (v.to(input_device) if torch.is_tensor(v) else v) for k, v in inputs.items()}

    with torch.inference_mode():
        outputs = model.generate(
            **inputs,
            max_new_tokens=MAX_NEW_TOKENS,
            do_sample=True,
            temperature=TEMPERATURE,
            top_p=TOP_P,
            repetition_penalty=REPETITION_PENALTY,
            eos_token_id=tokenizer.eos_token_id,
            pad_token_id=tokenizer.eos_token_id,
        )

    # ✅ Cắt phần prompt ra khỏi output
    gen_ids = outputs[0][inputs["input_ids"].shape[-1] :]
    text = tokenizer.decode(gen_ids, skip_special_tokens=True).strip()
    return text



@app.on_event("startup")
def on_startup():
    DATA_DIR.mkdir(parents=True, exist_ok=True)
    # Load model ngay khi start (để tránh gọi /v1/chat bị AssertionError rỗng)
    global model_ready, model_error
    try:
        _load_model()
    except Exception as e:
        model_ready = False
        model_error = f"{type(e).__name__}: {e}"
        print("[AI] ❌ Load model failed:\n" + traceback.format_exc())


@app.get("/healthz")
def healthz():
    return {
        "ok": True,
        "ready": bool(model_ready and model is not None and tokenizer is not None),
        "model": MODEL_ID,
        "cuda": torch.cuda.is_available(),
        "error": model_error,
        "time": datetime.utcnow().isoformat() + "Z",
    }

@app.post("/v1/chat", response_model=ChatResponse)
async def chat(req: ChatRequest):
    if not req.title.strip() and not req.body.strip():
        raise HTTPException(status_code=400, detail="title/body is required")

    request_id = str(uuid.uuid4())
    t0 = time.time()

    try:
        async with _generate_lock:
            # Lazy-load phòng trường hợp startup load fail / bị tắt
            if model is None or tokenizer is None:
                _load_model()
            answer = _generate_answer(req)
    except Exception as e:
        tb = traceback.format_exc()
        print(f"[AI] ❌ Generate failed (request_id={request_id}):\n{tb}")

        # response error rõ hơn (dev) + có request_id để đối chiếu log
        if DEBUG_STACKTRACE:
            detail = (
                f"Model generate failed: {type(e).__name__}: {e}\n"
                f"request_id={request_id}\n\n{tb}"
            )
        else:
            detail = f"Model generate failed: {type(e).__name__}: {e} (request_id={request_id})"

        raise HTTPException(status_code=500, detail=detail)

    disclaimer = "Gợi ý từ AI, vui lòng kiểm chứng trước khi dùng làm trả lời chính thức."
    _ = time.time() - t0

    return JSONResponse(
    content={
        "answer": answer,
        "disclaimer": disclaimer,
        "request_id": request_id,
    },
    media_type="application/json; charset=utf-8",
)



@app.post("/v1/feedback")
def feedback(req: FeedbackRequest):
    if not req.answer.strip():
        raise HTTPException(status_code=400, detail="answer is required")

    record = {
        "id": str(uuid.uuid4()),
        "ts": datetime.utcnow().isoformat() + "Z",
        "org_id": req.org_id,
        "question_id": req.question_id,
        "question": req.question,
        "answer": req.answer,
        "is_admin": req.is_admin,
        "meta": req.meta,
    }

    try:
        with FEEDBACK_PATH.open("a", encoding="utf-8") as f:
            f.write(json.dumps(record, ensure_ascii=False) + "\n")
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Write feedback failed: {e}")

    return {"ok": True, "saved": True}
