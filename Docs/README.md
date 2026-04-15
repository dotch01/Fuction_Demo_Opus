# Fuction_Demo_Opus — 專案總覽與上傳指南

## 專案簡介

Unity 6 作品集，包含 **5 個獨立 Demo**，全部採用 **純程式碼建構場景（Zero-Prefab）** 的架構風格，無任何 Prefab 或場景預設物件。

| Demo | 類型 | 場景 |
|------|------|------|
| **Space Explorer** | 2D 太空探索 + Firebase 雲端存檔 | `Demo.unity` |
| **Collectibles** | 2D 物品收集 + 背包 + HP/MP 系統 | `Demo.unity` (Demo2) |
| **Math Museum** | 3D 第一人稱數學博物館（50+ 展品） | `GameMath.unity` |
| **Shader Showcase** | 26 種 Shader 互動展示 | `Shader.unity` |
| **TalkBot** | AI 聊天角色（Gemini + Supabase RAG） | `TalkBotDemo.unity` |

**引擎版本：** Unity 6（URP 17.3.0）  
**渲染管線：** Universal Render Pipeline  
**輸入系統：** New Input System  
**目標平台：** WebGL（主要）、Windows、Android、macOS

---

## 面試作品上傳建議

### ✅ 應該上傳的部分

| 路徑 | 原因 |
|------|------|
| `Assets/Scripts/` | **核心程式碼**，展示你的架構能力與技術深度 |
| `Assets/Scenes/` | 場景檔案（`.unity`），面試官可直接開啟 |
| `Assets/Resources/` | 字型等運行時資源 |
| `Assets/Settings/` | URP 渲染管線設定 |
| `Assets/InputSystem_Actions.inputactions` | Input System 設定 |
| `Assets/WebGLTemplates/Firebase/index.html` | 自訂 WebGL 模板（展示前端整合能力） |
| `Assets/StreamingAssets/PlanetConfig.json` | 遊戲配置資料 |
| `Assets/StreamingAssets/SceneConfig.json` | 場景配置資料 |
| `Assets/StreamingAssets/StoryConfig.json` | 劇情配置資料 |
| `Assets/StreamingAssets/ItemConfig.json` | 道具配置資料 |
| `Assets/StreamingAssets/firestore.rules` | Firestore 安全規則（展示後端安全設計） |
| `Assets/StreamingAssets/Settings/` | 角色設定文本 |
| `Packages/manifest.json` | 套件依賴清單 |
| `ProjectSettings/` | Unity 專案設定 |
| `Tools/` | 資料表格轉換工具（展示工具鏈能力） |
| `Output/` | 建置輸出（可選，提供可直接試玩的版本） |
| `Docs/` | 本架構文件資料夾 |

### ❌ 不應上傳的部分（隱私 / 安全 / 體積）

| 路徑 | 原因 | 風險等級 |
|------|------|----------|
| `Assets/google-services.json` | 含 Firebase API Key + OAuth Client ID | 🔴 高 |
| `Assets/StreamingAssets/firebase-config.json` | 含 Firebase Web API Key | 🟡 中（客戶端金鑰，但仍應限制） |
| `Assets/StreamingAssets/google-services-desktop.json` | 同上 | 🟡 中 |
| `Assets/Scripts/TalkDemo/Config/ApiConfig.asset` | **含 Gemini API Key 明文** | 🔴 高 |
| `Assets/Scripts/TalkDemo/Supabase Config.asset` | 含 Supabase URL + JWT anon key | 🔴 高 |
| `Library/` | Unity 快取（巨大且可重建） | 體積 |
| `Temp/` | 暫存檔案 | 體積 |
| `Logs/` | 日誌 | 無關 |
| `UserSettings/` | 個人 IDE 設定 | 無關 |
| `ProfilerCaptures/` | 效能擷取 | 無關 |
| `*.csproj` / `*.slnx` | IDE 自動生成 | 可重建 |
| `Assets/Firebase/` | Firebase SDK 二進位（可透過 Package Manager 還原） | 體積 |
| `Assets/ExternalDependencyManager/` | Google EDM4U 外掛 | 體積 |
| `Assets/Plugins/` | 第三方外掛二進位 | 體積 / 授權 |

### 📝 建議的 .gitignore 補充

在現有 `ignore.conf` 基礎上，**必須額外排除**以下敏感檔案：

```gitignore
# === 敏感資料 ===
Assets/google-services.json
Assets/google-services.json.meta
Assets/StreamingAssets/firebase-config.json
Assets/StreamingAssets/firebase-config.json.meta
Assets/StreamingAssets/google-services-desktop.json
Assets/StreamingAssets/google-services-desktop.json.meta
Assets/Scripts/TalkDemo/Config/ApiConfig.asset
Assets/Scripts/TalkDemo/Config/ApiConfig.asset.meta
Assets/Scripts/TalkDemo/Supabase Config.asset
Assets/Scripts/TalkDemo/Supabase Config.asset.meta

# === 大型二進位 / 可還原 ===
Assets/Firebase/
Assets/ExternalDependencyManager/
Assets/Plugins/
Output/**/*.zip
```

### 🔐 安全建議

1. **立即輪換** Gemini API Key 和 Supabase anon key（若曾公開推送）
2. Firebase 客戶端金鑰透過 **App Check** 和 **API 限制** 保護
3. 提供 `.env.example` 或 `README` 說明如何填入 API Key：

```
# .env.example
GEMINI_API_KEY=your-key-here
SUPABASE_URL=https://your-project.supabase.co
SUPABASE_ANON_KEY=your-anon-key
FIREBASE_API_KEY=your-firebase-key
```

---

## 架構文件索引

| 文件 | 內容 |
|------|------|
| [01_SpaceExplorer_Demo.md](01_SpaceExplorer_Demo.md) | 太空探索 Demo — 架構、系統設計、Firebase 整合 |
| [02_Collectibles_Demo2.md](02_Collectibles_Demo2.md) | 物品收集 Demo — 程式化場景、物理、背包系統 |
| [03_MathMuseum_MathDemo.md](03_MathMuseum_MathDemo.md) | 數學博物館 — 50+ 展品、自訂渲染、挑戰系統 |
| [04_ShaderShowcase_Shader.md](04_ShaderShowcase_Shader.md) | Shader 展示 — 26 種 HLSL Shader 技術解析 |
| [05_TalkBot_TalkDemo.md](05_TalkBot_TalkDemo.md) | AI 聊天機器人 — Gemini API、RAG、情緒系統 |
| [06_Common_Utilities.md](06_Common_Utilities.md) | 共用工具模組 |

---

## Tools 工具鏈總覽

位於 `Tools/` 資料夾，提供企劃 Excel → JSON 的資料轉換管線。

| 工具 | 對應 Demo | 輸入 | 輸出 |
|------|-----------|------|------|
| `generate_planets.py` | Space Explorer | 無（隨機生成） | `PlanetConfig.json` |
| `generate_planets_from_excel.py` | Space Explorer | Excel（星域+行星） | `PlanetConfig.json` |
| `generate_scene.py` | Space Explorer | Excel（場景物件） | `SceneConfig.json` |
| `generate_story.py` | Space Explorer | Excel（劇情章節） | `StoryConfig.json` |
| `generate_items_from_excel.py` | Collectibles | Excel（道具） | `ItemConfig.json` |
| `upload_settings_to_supabase.py` | TalkBot | `.txt` 文本 | Supabase 向量資料庫 |

詳細的 Excel 欄位格式請參考各 Demo 的技術文件。
