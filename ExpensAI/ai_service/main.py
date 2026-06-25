from fastapi import FastAPI, File, UploadFile, HTTPException, Request
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse
import shutil
import uuid
import os
import pandas as pd

app = FastAPI(docs_url=None if os.getenv("ENVIRONMENT") == "production" else "/docs",
              redoc_url=None if os.getenv("ENVIRONMENT") == "production" else "/redoc")

# CORS Setup
app.add_middleware(
    CORSMiddleware,
    allow_origins=[os.getenv("ALLOWED_ORIGIN", "https://meuapp.com")],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# CSP and Security Headers Middleware
@app.middleware("http")
async def add_security_headers(request: Request, call_next):
    response = await call_next(request)
    response.headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self'; object-src 'none'; frame-ancestors 'none';"
    response.headers["X-Content-Type-Options"] = "nosniff"
    response.headers["X-Frame-Options"] = "DENY"
    response.headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload"
    return response

# Simple in‑memory categorization rules (could be replaced by ML model)
CATEGORY_MAP = {
    "ifood": "Alimentação",
    "rappi": "Alimentação",
    "uber": "Transporte",
    "pix": "Transferência",
    # add more rules as needed
}

def categorize(description: str) -> str:
    desc = description.lower()
    for keyword, category in CATEGORY_MAP.items():
        if keyword in desc:
            return category
    return "Outros"

@app.post("/process")
async def process_file(file: UploadFile = File(...)):
    # Save uploaded file to a temporary location
    ext = os.path.splitext(file.filename)[1].lower()
    temp_path = os.path.join("/tmp", f"{uuid.uuid4()}{ext}")
    try:
        with open(temp_path, "wb") as buffer:
            shutil.copyfileobj(file.file, buffer)
    finally:
        await file.close()

    # Very simple parser: assume CSV with columns Date, Amount, Description
    try:
        df = pd.read_csv(temp_path)
    except Exception as e:
        raise HTTPException(status_code=400, detail=f"Failed to parse CSV: {e}")

    # Apply categorization
    df["Category"] = df["Description"].apply(categorize)

    # Return JSON result (in real case, you would persist to DB)
    result = df.to_dict(orient="records")
    return {"transactions": result}
