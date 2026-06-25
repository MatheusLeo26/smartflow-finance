from fastapi import FastAPI, File, UploadFile, HTTPException
import shutil
import uuid
import os
import pandas as pd
import json

app = FastAPI()

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
