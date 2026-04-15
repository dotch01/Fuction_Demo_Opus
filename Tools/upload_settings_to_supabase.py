"""
upload_settings_to_supabase.py — 設定文字向量化並寫入 Supabase

功能：
  讀取指定資料夾下所有 .txt 檔，用 Gemini text-embedding-004 轉成向量，
  然後 upsert 進 Supabase 的 settings 資料表。

  每次執行會以 filename 為鍵值做 upsert（重複執行不會重複新增）。

使用方式：
  python upload_settings_to_supabase.py

執行前請填入下方 ===設定區=== 的三個值。
"""

import os
import sys
import requests

# ===設定區（只需填這裡）===
GEMINI_API_KEY = "AIzaSyCQe7UO8DvvuT5RlAPDcMknYtyed0_RL6Q"
SUPABASE_URL   = "https://ppdsfcczadgaqrxjptaj.supabase.co"
SUPABASE_KEY   = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InBwZHNmY2N6YWRnYXFyeGpwdGFqIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc3NTE0NzM5MiwiZXhwIjoyMDkwNzIzMzkyfQ.XDznxtEl77LVXZg6WcdEetl-vmFzti4TlxSHHINWQFY"        # service_role key（只在本機跑）
SETTINGS_DIR   = "../Assets/StreamingAssets/Settings"  # .txt 資料夾路徑（相對此腳本）
# ========================

EMBEDDING_URL = (
    "https://generativelanguage.googleapis.com/v1beta/models/"
    f"gemini-embedding-001:embedContent?key={GEMINI_API_KEY}"
)

SUPABASE_UPSERT_URL = f"{SUPABASE_URL}/rest/v1/settings"
SUPABASE_HEADERS = {
    "apikey": SUPABASE_KEY,
    "Authorization": f"Bearer {SUPABASE_KEY}",
    "Content-Type": "application/json",
    "Prefer": "resolution=merge-duplicates",   # upsert：重複 filename 自動覆蓋
}


def get_embedding(text):
    """呼叫 Gemini Embedding API，回傳 float 向量。"""
    body = {
        "model": "models/gemini-embedding-001",
        "content": {"parts": [{"text": text}]},
        "output_dimensionality": 768
    }
    res = requests.post(EMBEDDING_URL, json=body, timeout=30)
    if res.status_code != 200:
        print(f"  [錯誤] Embedding API 失敗：{res.status_code} {res.text[:200]}")
        return None
    return res.json()["embedding"]["values"]


def upsert_row(filename, content, embedding):
    """把一筆資料 upsert 進 Supabase。"""
    row = {
        "filename": filename,
        "content": content,
        "embedding": embedding,
    }
    res = requests.post(
        SUPABASE_UPSERT_URL,
        json=row,
        headers=SUPABASE_HEADERS,
        timeout=30,
    )
    if res.status_code not in (200, 201, 204):
        print(f"  [錯誤] Supabase 寫入失敗：{res.status_code} {res.text[:200]}")
        return False
    return True


def main():
    # 解析腳本相對路徑
    script_dir = os.path.dirname(os.path.abspath(__file__))
    settings_path = os.path.normpath(os.path.join(script_dir, SETTINGS_DIR))

    if not os.path.isdir(settings_path):
        print(f"[錯誤] 找不到設定資料夾：{settings_path}")
        print("請確認 SETTINGS_DIR 路徑正確，或手動建立該資料夾並放入 .txt 檔。")
        sys.exit(1)

    # 收集所有 .txt
    txt_files = []
    for root, _, files in os.walk(settings_path):
        for f in files:
            if f.endswith(".txt"):
                txt_files.append(os.path.join(root, f))

    if not txt_files:
        print(f"[警告] 資料夾 {settings_path} 裡沒有 .txt 檔。")
        sys.exit(0)

    print(f"找到 {len(txt_files)} 個 .txt 檔，開始處理...\n")

    success = 0
    failed = 0

    for filepath in txt_files:
        filename = os.path.relpath(filepath, settings_path).replace("\\", "/")
        print(f"處理：{filename}")

        with open(filepath, "r", encoding="utf-8") as f:
            content = f.read().strip()

        if not content:
            print("  [跳過] 檔案是空的")
            continue

        embedding = get_embedding(content)
        if embedding is None:
            failed += 1
            continue

        ok = upsert_row(filename, content, embedding)
        if ok:
            print(f"  ✓ 寫入成功（{len(embedding)} 維向量）")
            success += 1
        else:
            failed += 1

    print(f"\n完成。成功 {success} 筆 / 失敗 {failed} 筆。")
    if failed > 0:
        print("失敗的項目請檢查上方錯誤訊息後重新執行，重複執行不會產生重複資料。")


if __name__ == "__main__":
    main()
