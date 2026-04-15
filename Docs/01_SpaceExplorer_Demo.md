# 01 — Space Explorer Demo（太空探索）

## 概述

2D 俯視角太空探索遊戲，玩家駕駛太空船在無限循環的宇宙中探索行星、執行任務、推進劇情。  
整合 Firebase Authentication 與 Firestore 雲端存檔，支援 WebGL / Windows / Android 多平台。

**場景：** `Demo.unity`  
**腳本路徑：** `Assets/Scripts/Demo/`  
**資料路徑：** `Assets/StreamingAssets/PlanetConfig.json`, `SceneConfig.json`, `StoryConfig.json`

---

## 系統架構圖

```
GameSetup (Composition Root)
│
├── DataLoader ─── PlanetConfigLoader
│              ├── SceneConfigLoader
│              └── StoryConfigLoader
│
├── GameFlowManager (State Machine)
│   ├── State: Spaceship ←→ Dialogue ←→ StarMap
│   └── StoryConfig 觸發章節劇情
│
├── PlanetFactory (Singleton)
│   ├── 從 PlanetConfig.json 生成行星 GameObject
│   ├── 分為 Normal / Filter 兩組
│   └── LateUpdate() 環形世界循環定位
│
├── FilterManager ─── 切換 Normal / Filter 行星可見性
│
├── CameraController
│   ├── WASD + 滑鼠拖曳虛擬搖桿
│   ├── 方向指示線 (LineRenderer)
│   └── 彈性跟隨 + 世界邊界循環
│
├── ScanSystem
│   ├── 十字準星掃描偵測（螢幕空間矩形判定）
│   ├── 空白鍵觸發掃描
│   └── 通知 QuestManager + SaveManager
│
├── QuadrantManager ─── 座標取模判定象限 (1-4)
├── StarSystemManager ─── 圓形區域判定星系歸屬
│
├── QuestManager (觀察者模式)
│   ├── 線性任務鏈（questOrder）
│   ├── QuestLocator 座標/象限/星系/濾鏡條件判定
│   └── OnQuestUpdated 事件
│
├── SaveManager (策略模式)
│   ├── ISaveProvider 介面
│   ├── LocalSaveProvider — JSON + SHA256 校驗
│   ├── CloudSaveProvider — Firebase Firestore
│   └── 時間戳衝突解決
│
├── AuthManager (Singleton)
│   ├── 匿名登入 → Email 連結帳號
│   ├── Google Sign-In
│   ├── WebGL jslib 橋接
│   └── OnAuthStateChanged 事件
│
├── SpaceshipView ─── 從 SceneConfig.json 建構太空船內場景
│
└── UI Layer
    ├── HUDManager ─── 座標/象限/星系即時顯示
    ├── DialogueUI ─── 視覺小說風格對話框
    ├── PlanetDetailUI ─── 行星詳情面板（半徑/質量/溫度/含水/地球相似度）
    ├── QuestPanelUI ─── 任務列表/詳情雙模式面板
    ├── QuestHintUI ─── 延遲出現的任務方向箭頭
    └── AccountUI ─── 帳號管理面板
```

---

## 設計模式

| 模式 | 應用位置 | 說明 |
|------|---------|------|
| **Composition Root** | `GameSetup` | 場景啟動入口，程式碼建構所有 Manager / UI / 相機 |
| **Singleton** | 幾乎所有 Manager | 透過靜態 `Instance` 存取 |
| **State Machine** | `GameFlowManager` | 三種狀態：Spaceship / Dialogue / StarMap |
| **Observer** | `QuestManager`, `AuthManager` | 事件通知 UI 更新與系統串接 |
| **Strategy** | `ISaveProvider` | 本地 / 雲端存檔策略可替換 |
| **Factory** | `PlanetFactory` | 從 JSON 資料批量產生行星物件 |
| **Service Locator / Cache** | `DataLoader` + 各 Loader | Preload → Cache → Load 模式 |
| **Bridge** | `WebGLFirebaseInterop` | WebGL 透過 jslib 呼叫 JS Firebase SDK |
| **Data-Driven** | `PlanetConfig.json` 等 | 遊戲內容由外部 JSON 驅動 |

---

## 技術亮點

### 1. 環形世界（Toroidal World Wrap）
- `PlanetFactory.LateUpdate()` 中，行星位置根據相機座標取模，實現無縫循環
- `CameraController` 同步處理相機位置循環

### 2. WebGL Firebase 整合
- **原生 SDK 無法在 WebGL 使用**，故透過 `WebGLFirebaseInterop.cs` 定義 P/Invoke 橋接
- `Assets/WebGLTemplates/Firebase/index.html` 載入 Firebase JS SDK (v10.12.0)
- C# → jslib → JavaScript Firebase SDK 的完整通訊鏈
- 條件編譯 `#if UNITY_WEBGL` / `#if FIREBASE_AUTH` 處理平台差異

### 3. 存檔完整性驗證
- `GameSaveData` 包含 SHA256 checksum
- `LocalSaveProvider` 存檔時計算、讀檔時驗證
- 防止存檔被手動竄改

### 4. 雲端存檔衝突解決
- `SaveManager` 比較本地與雲端的 `lastSaveTime`
- 自動取用較新的存檔版本

### 5. 100% 程式碼建構
- 無任何 Prefab 或場景預設物件
- `GameSetup` 在 `Start()` 中建構完整場景：相機、行星、UI、Manager
- 展示對 Unity API 的深度理解

---

## 資料流

```
StreamingAssets/*.json
       │
       ▼
 DataLoader (UnityWebRequest, async)
       │
       ▼
 PlanetConfigLoader / SceneConfigLoader / StoryConfigLoader (Static Cache)
       │
       ▼
 PlanetFactory / SpaceshipView / GameFlowManager (消費資料)
       │
       ▼
 ScanSystem → QuestManager → SaveManager
       │                        │
       ▼                        ▼
 QuestPanelUI              LocalSaveProvider (JSON + SHA256)
 QuestHintUI               CloudSaveProvider (Firestore)
```

---

## 檔案清單

| 檔案 | 職責 |
|------|------|
| `GameSetup.cs` | 場景初始化入口 |
| `GameFlowManager.cs` | 遊戲狀態管理 |
| `PlanetFactory.cs` | 行星生成與世界循環 |
| `FilterManager.cs` | 行星篩選切換 |
| `ScanSystem.cs` | 掃描系統 |
| `CameraController.cs` | 相機控制 |
| `QuadrantManager.cs` | 象限判定 |
| `StarSystemManager.cs` | 星系判定 |
| `Star.cs` | 行星資料組件 |
| `SpaceshipView.cs` | 太空船場景 |
| `HUDManager.cs` | HUD 更新 |
| `Data/PlanetData.cs` | 行星資料結構 |
| `Data/SceneData.cs` | 場景資料結構 |
| `Data/StoryData.cs` | 劇情資料結構 |
| `Data/GameSaveData.cs` | 存檔資料 + SHA256 |
| `Data/DataLoader.cs` | 非同步 JSON 載入器 |
| `Data/PlanetConfigLoader.cs` | 行星配置快取 |
| `Data/SceneConfigLoader.cs` | 場景配置快取 |
| `Data/StoryConfigLoader.cs` | 劇情配置快取 |
| `Quest/QuestManager.cs` | 任務管理 |
| `Quest/QuestPanelUI.cs` | 任務面板 UI |
| `Quest/QuestLocator.cs` | 任務定位條件判定 |
| `Save/ISaveProvider.cs` | 存檔介面 |
| `Save/LocalSaveProvider.cs` | 本地存檔 |
| `Save/CloudSaveProvider.cs` | 雲端存檔 (Firestore) |
| `Save/SaveManager.cs` | 存檔協調器 |
| `Save/AuthManager.cs` | 認證管理 |
| `Save/WebGLFirebaseInterop.cs` | WebGL Firebase 橋接 |
| `UI/DialogueUI.cs` | 對話框 |
| `UI/AccountUI.cs` | 帳號面板 |
| `UI/PlanetDetailUI.cs` | 行星詳情面板 |
| `UI/QuestHintUI.cs` | 任務方向提示 |

---

## Tools — 資料表格轉換

本 Demo 使用以下工具從 Excel 生成 JSON 配置檔：

### 1. `Tools/generate_planets.py` — 隨機行星生成
- **用途：** 批量隨機產生行星配置
- **輸入：** 無（硬編碼星系資料）
- **輸出：** `Assets/StreamingAssets/PlanetConfig.json`
- **依賴：** 無（Python 標準庫）

### 2. `Tools/generate_planets_from_excel.py` — Excel → PlanetConfig.json
- **用途：** 讓企劃從 Excel 管理行星資料
- **依賴：** `pip install openpyxl`
- **使用：** `python generate_planets_from_excel.py planets.xlsx ../Assets/StreamingAssets/PlanetConfig.json`

#### Excel 格式

**Sheet「星域」：**

| 欄 | 欄位 | 範例 |
|----|------|------|
| A | name | `GEVURAH` |
| B | quadrant | `1` |
| C | center_x | `25` |
| D | center_y | `75` |
| E | radius | `18` |

**Sheet「行星」：**

| 欄 | 欄位 | 範例 |
|----|------|------|
| A | id | `EMETH-100` |
| B | defaultName | `Nono_01` (非任務留空) |
| C | description | `此行星...` |
| D | pos_x | `34.89` |
| E | pos_y | `65.96` |
| F | scale | `0.32` |
| G | radius | `4118` |
| H | mass | `29182` |
| I | temperature | `92` |
| J | waterPercent | `69` |
| K | earthSimilarity | `13.89` |
| L | isQuestTarget | `TRUE` / `FALSE` |
| M | filterOnly | `TRUE` / `FALSE` |
| N | questOrder | `1` (非任務填 0) |

### 3. `Tools/generate_scene.py` — Excel → SceneConfig.json
- **用途：** 太空船場景物件與對話資料
- **依賴：** `pip install openpyxl`

**Sheet「場景物件」：**

| 欄 | 欄位 | 說明 |
|----|------|------|
| A | scene_id | 場景 ID |
| B | object_id | 物件 ID（重複行合併為多行對話） |
| C | label | 物件標籤 |
| D-E | pos_x, pos_y | 位置 |
| F-G | width, height | 尺寸 |
| H-J | color_r/g/b | 顏色 (0~255) |
| K | speaker | 說話者 |
| L | portrait | 頭像 |
| M | text | 對話文字 |

### 4. `Tools/generate_story.py` — Excel → StoryConfig.json
- **用途：** 劇情章節與對話
- **依賴：** `pip install openpyxl`

**Sheet1 欄位：**

| 欄 | 欄位 | 說明 |
|----|------|------|
| A | chapter_id | 章節 ID（重複行合併） |
| B | triggerAfterQuest | 觸發任務序號 (0=開場) |
| C | speaker | 說話者 |
| D | portrait | 頭像 |
| E | text | 對話文字 |
| F | onComplete | `openStarMap` / `returnToShip`（僅最後一行） |

---

## 第三方依賴

| 服務 | 用途 |
|------|------|
| **Firebase Authentication** | 匿名登入、Email 登入、Google Sign-In |
| **Firebase Firestore** | 雲端存檔 |
| **Unity New Input System** | 跨平台輸入 |
| **URP** | 渲染管線 |
