# Fuction Demo Opus — Unity 6 作品集

Unity 6 作品集專案，包含 5 個獨立 Demo，全部採用 **純程式碼建構場景（Zero-Prefab）** 的架構風格。

## Demo 一覽

| # | Demo | 說明 | 技術亮點 |
|---|------|------|---------|
| 1 | **[Space Explorer](https://dotch01.itch.io/opus1-demo)**<br>[![](https://cdn.phototourl.com/free/2026-04-20-0f7aa7c7-d134-47c0-9b51-baa58f702341.png)](https://dotch01.itch.io/opus1-demo) | 2D 太空探索 + 雲端存檔 | Firebase Auth/Firestore、WebGL jslib 橋接、環形世界、SHA256 存檔校驗 |
| 2 | **[Collectibles](https://dotch01.itch.io/inventorysystem)**<br>[![](https://cdn.phototourl.com/free/2026-04-20-3b449979-fc40-4fb1-9333-afaad5198d5a.png)](https://dotch01.itch.io/inventorysystem) | 2D 物品收集 + 背包 + HP/MP | JSON 驅動道具系統、格子式背包、消耗品/任務道具/收藏品 |
| 3 | **[Math Museum](https://dotch01.itch.io/gamemath)**<br>[![](https://cdn.phototourl.com/free/2026-04-20-18c6fe7f-3a71-415f-979e-0aeef0a51d4c.png)](https://dotch01.itch.io/gamemath) | 3D 數學博物館（50+ 展品） | Template Method 展品架構、物件池化線段渲染、3D 拖曳互動 |
| 4 | **[Shader Showcase](https://dotch01.itch.io/shader-demo)**<br>[![](https://cdn.phototourl.com/free/2026-04-20-b23a7417-9737-4e4f-aab7-30427b4f8eb9.png)](https://dotch01.itch.io/shader-demo) | 26 種 HLSL Shader 展示 | Ray Marching 體積雲、Portal Stencil、POM、SSR、即時參數調整 |
| 5 | **[TalkBot](https://dotch01.itch.io/talkbot-demo)**<br>[![](https://cdn.phototourl.com/free/2026-04-20-e3c27a91-85b2-49d0-93d9-42dd2b2c5b7b.png)](https://dotch01.itch.io/talkbot-demo) | AI 聊天角色 | Gemini API + Supabase RAG 向量搜尋、情緒驅動動畫、透明桌面寵物 |

## 技術棧

- **引擎：** Unity 6（URP 17.3.0）
- **語言：** C# / HLSL
- **輸入系統：** New Input System
- **渲染管線：** Universal Render Pipeline
- **雲端：** Firebase Auth + Firestore、Google Gemini API、Supabase (pgvector)
- **平台：** WebGL、Windows、Android、macOS
- **工具鏈：** Python（Excel → JSON 轉換管線）

## 架構特色

- **Zero-Prefab** — 場景中無任何預設物件，所有 GameObject/UI/材質完全由程式碼建構
- **Data-Driven** — 遊戲內容由 JSON 配置檔驅動，搭配 Excel 工具鏈管理
- **模組化設計** — 各 Demo 獨立且共用 Common 工具層
- **設計模式** — Singleton、Observer、Strategy、Template Method、Factory、Object Pool、State Machine

## 專案結構

```
Assets/
├── Scripts/
│   ├── Common/        # 共用工具（字型、教學提示）
│   ├── Demo/          # Space Explorer（太空探索）
│   ├── Demo2/         # Collectibles（物品收集 + 背包）
│   ├── MathDemo/      # Math Museum（數學博物館）
│   ├── Shader/        # Shader Showcase（Shader 展示）
│   └── TalkDemo/      # TalkBot（AI 聊天）
├── StreamingAssets/   # JSON 配置檔
├── Scenes/            # Unity 場景檔
└── Settings/          # URP 渲染管線設定

Tools/                 # Excel → JSON 轉換工具（Python）
Docs/                  # 詳細技術架構文件
Output/                # 建置輸出（WebGL / Win）
```

## 詳細技術文件

請參閱 [`Docs/`](Docs/README.md) 資料夾：

| 文件 | 內容 |
|------|------|
| [專案總覽與上傳指南](Docs/README.md) | 完整上傳建議與安全指南 |
| [Space Explorer](Docs/01_SpaceExplorer_Demo.md) | 太空探索 — Firebase 整合、存檔系統 |
| [Collectibles](Docs/02_Collectibles_Demo2.md) | 背包系統 — HP/MP、JSON 驅動道具 |
| [Math Museum](Docs/03_MathMuseum_MathDemo.md) | 50+ 數學展品、自訂渲染系統 |
| [Shader Showcase](Docs/04_ShaderShowcase_Shader.md) | 26 種 HLSL Shader 技術解析 |
| [TalkBot](Docs/05_TalkBot_TalkDemo.md) | Gemini API、RAG、情緒動畫 |

## 環境設定

### 必要條件

- Unity 6 (6000.x)
- Universal Render Pipeline 17.x
- Firebase SDK（透過 Unity Package Manager）

### API 金鑰設定

本專案需要以下 API 金鑰（出於安全考量不包含在 repo 中）：

| 服務 | 設定位置 | 說明 |
|------|---------|------|
| Firebase | `Assets/StreamingAssets/firebase-config.json` | Web 端 Firebase 配置 |
| Firebase (Android) | `Assets/google-services.json` | Android Firebase 配置 |
| Gemini API | `Assets/Scripts/TalkDemo/Config/ApiConfig.asset` | ScriptableObject，在 Inspector 填入 |
| Supabase | `Assets/Scripts/TalkDemo/Supabase Config.asset` | ScriptableObject，在 Inspector 填入 |

請參考各服務文件取得金鑰，然後建立對應的設定檔。

## 授權

本專案為個人作品集，僅供面試參考。
