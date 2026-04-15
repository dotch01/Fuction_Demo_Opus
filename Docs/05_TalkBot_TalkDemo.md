# 05 — TalkBot Demo（AI 聊天機器人）

## 概述

AI 驅動的角色聊天系統，使用 **Google Gemini API** 生成對話，結合 **Supabase 向量搜尋 (RAG)** 提供角色知識庫上下文。  
角色根據對話情緒產生動態動畫，支援透明無邊框視窗模式（桌面寵物風格）。

**場景：** `TalkBotDemo.unity`  
**腳本路徑：** `Assets/Scripts/TalkDemo/`

---

## 系統架構圖

```
TalkDemoController (Scene Orchestrator)
│
├── Chat UI (ChatUIBuilder)
│   ├── 角色圖像 (RectTransform)
│   ├── 對話泡泡 (ScrollRect)
│   ├── 輸入欄 (InputField)
│   └── 送出按鈕
│
├── RAG Pipeline (Coroutine-based)
│   ├── 1. 使用者輸入
│   ├── 2. Gemini Embedding API → 向量化
│   ├── 3. Supabase RPC (search_settings) → 相似文本
│   ├── 4. 注入上下文到 System Prompt
│   ├── 5. Gemini generateContent → 回覆 + 情緒標籤
│   └── 6. 解析 [EMOTION] 標籤 → 驅動動畫
│
├── ConversationManager (Service)
│   ├── 對話歷史管理 (最近 N 則)
│   ├── Gemini API JSON 建構
│   ├── 回覆解析 + 情緒提取
│   └── 隨機話題產生 (自動閒聊)
│
├── CharacterAnimationHandler
│   ├── 情緒動畫策略
│   │   ├── Happy → 跳躍
│   │   ├── Sad → 下沉
│   │   ├── Surprised → 抖動
│   │   ├── Angry → 搖晃
│   │   ├── Calm → 緩慢傾斜
│   │   └── Neutral → 無特效
│   └── 閒置漂浮動畫 (sin 波)
│
├── TransparentWindowHelper (Platform Abstraction)
│   ├── Windows: user32.dll + dwmapi.dll P/Invoke
│   │   ├── 無邊框 (WS_POPUP)
│   │   ├── 透明 (DwmExtendFrameIntoClientArea)
│   │   ├── 永遠置頂 (HWND_TOPMOST)
│   │   └── 滑鼠穿透 (WS_EX_LAYERED + WS_EX_TRANSPARENT)
│   └── macOS: 部分支援
│
└── Config Layer (ScriptableObjects)
    ├── GeminiApiConfig ─── API Key + Endpoint URL
    ├── SupabaseConfig ─── URL + Anon Key + RAG 開關
    └── CharacterPromptConfig ─── System Prompt + 話題列表 + 歷史限制
```

---

## RAG (Retrieval-Augmented Generation) 流程

```
使用者輸入: "你最喜歡去哪裡？"
         │
         ▼
┌─────────────────────────┐
│ Gemini Embedding API    │
│ gemini-embedding-001    │
│ → 768 維向量            │
└────────┬────────────────┘
         │
         ▼
┌─────────────────────────┐
│ Supabase RPC            │
│ search_settings(        │
│   query_embedding,      │
│   match_count: 3        │
│ )                       │
│ → Top 3 相似文本片段    │
└────────┬────────────────┘
         │
         ▼
┌─────────────────────────┐
│ System Prompt 注入      │
│ "以下是角色背景資料：   │
│  {RAG 結果}             │
│  請以角色身份回覆。"    │
└────────┬────────────────┘
         │
         ▼
┌─────────────────────────┐
│ Gemini generateContent  │
│ gemini-2.0-flash        │
│ → "[HAPPY] 我最喜歡     │
│    去海邊了！"           │
└────────┬────────────────┘
         │
    ┌────┴────┐
    ▼         ▼
[HAPPY]    "我最喜歡去海邊了！"
    │         │
    ▼         ▼
跳躍動畫   顯示在對話泡泡
```

---

## 設計模式

| 模式 | 應用位置 | 說明 |
|------|---------|------|
| **Mediator** | `TalkDemoController` | 協調 UI、API、動畫、RAG 各模組 |
| **Builder** | `ChatUIBuilder` | 靜態方法程式碼建構完整聊天 UI |
| **Service** | `ConversationManager` | 純 C# 類，管理對話邏輯 |
| **Strategy** | `CharacterAnimationHandler` | 根據情緒類型選擇不同動畫 |
| **ScriptableObject Config** | 三個 Config 類 | 可在 Inspector 調整的設定資料 |
| **Platform Abstraction** | `TransparentWindowHelper` | Windows / macOS 不同實作 |
| **Coroutine Async** | 全域 | Unity Coroutine 處理非同步 API 呼叫 |

---

## 技術亮點

### 1. RAG 向量搜尋整合
- 使用 Gemini `gemini-embedding-001` 模型將使用者輸入向量化
- Supabase PostgreSQL + pgvector 擴展執行向量相似度搜尋
- 透過 RPC 呼叫 `search_settings` 函數
- 結果注入 System Prompt，讓 AI 回覆更符合角色設定

### 2. 情緒解析與動畫系統
```
API 回覆格式: "[EMOTION_TAG] 對話內容"

EmotionType enum:
  Neutral, Happy, Sad, Surprised, Angry, Calm

每種情緒對應 Coroutine 動畫:
  Happy     → Y 軸跳躍 (上下彈跳)
  Sad       → Y 軸下沉 + 縮小
  Surprised → 快速抖動 (Random offset)
  Angry     → Z 軸劇烈搖晃
  Calm      → 緩慢左右傾斜
```

### 3. 透明視窗桌面寵物
```csharp
// Windows P/Invoke
[DllImport("user32.dll")] SetWindowLong(hwnd, GWL_STYLE, WS_POPUP);
[DllImport("dwmapi.dll")] DwmExtendFrameIntoClientArea(hwnd, margins);
[DllImport("user32.dll")] SetWindowPos(hwnd, HWND_TOPMOST, ...);
```
- 無邊框 + 背景透明 → 角色浮現在桌面上
- 永遠置頂 → 桌面寵物效果
- 可選滑鼠穿透模式

### 4. 自動閒聊機制
- `CharacterPromptConfig` 定義閒聊話題列表與時間間隔
- `ConversationManager` 根據最近對話上下文產生隨機話題
- 定時器觸發自動對話，模擬角色主動說話

---

## 檔案清單

### 主控制器
| 檔案 | 職責 |
|------|------|
| `TalkDemoController.cs` | 場景協調器 — API 呼叫、RAG、情緒、UI 串接 |

### 模組 (Modules/)
| 檔案 | 職責 |
|------|------|
| `Modules/ConversationManager.cs` | 對話歷史、API JSON 建構、回覆解析 |
| `Modules/ChatUIBuilder.cs` | 程式碼建構聊天 UI |
| `Modules/CharacterAnimationHandler.cs` | 情緒驅動動畫 |
| `Modules/TransparentWindowHelper.cs` | 透明無邊框視窗 (Win32 P/Invoke) |

### 設定 (Config/)
| 檔案 | 職責 |
|------|------|
| `Config/GeminiApiConfig.cs` | ScriptableObject: API Key + 端點 |
| `Config/SupabaseConfig.cs` | ScriptableObject: Supabase 連線 + RAG 設定 |
| `Config/CharacterPromptConfig.cs` | ScriptableObject: System Prompt + 話題 |

### 資料
| 檔案 | 職責 |
|------|------|
| `EmotionType.cs` | 情緒類型 enum |

---

## 第三方依賴

| 服務 | 用途 | 模型 |
|------|------|------|
| **Google Gemini API** | 文本生成 | `gemini-2.0-flash` |
| **Google Gemini Embedding** | 向量嵌入 | `gemini-embedding-001` |
| **Supabase** | 向量搜尋 (pgvector) | PostgreSQL RPC |

---

## Tools — 資料上傳

### `Tools/upload_settings_to_supabase.py`
- **用途：** 將角色設定文本上傳至 Supabase 向量資料庫供 RAG 使用
- **輸入：** `Assets/StreamingAssets/Settings/*.txt` 中的文本檔案
- **流程：**
  1. 讀取每個 `.txt` 檔案內容
  2. 呼叫 Gemini `gemini-embedding-001` 產生 768 維向量
  3. Upsert 至 Supabase `settings` 表（`filename`, `content`, `embedding`）
- **依賴：** `pip install requests`
- **使用：** `python upload_settings_to_supabase.py`

> ⚠️ 此工具包含硬編碼的 Gemini API Key 和 Supabase service_role Key。  
> 上傳前請將金鑰替換為環境變數讀取方式。

### 文本格式

角色設定文本為純 `.txt` 檔案，放置於 `Assets/StreamingAssets/Settings/` 目錄下。  
每個檔案代表一個角色或設定主題，內容會被完整嵌入為一筆向量記錄。

---

## 安全注意事項

| 檔案 | 敏感內容 | 處理建議 |
|------|---------|---------|
| `Config/ApiConfig.asset` | Gemini API Key 明文 | **必須排除於版控** |
| `Supabase Config.asset` | Supabase URL + JWT | **必須排除於版控** |
| `GeminiApiConfig.cs` | 預設值提示文字（安全） | 可上傳 |
| `SupabaseConfig.cs` | 預設值佔位符（安全） | 可上傳 |
