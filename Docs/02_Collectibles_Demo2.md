# 02 — Collectibles Demo（物品收集 + 背包系統）

## 概述

2D 俯視角物品收集遊戲，玩家在程式化生成的競技場中移動，收集四種分類的道具：  
**貨幣、消耗品（HP/MP 藥水）、任務道具、收藏品**。  
支援 HP/MP 狀態系統、完整背包 UI（B 鍵開啟）、道具使用與詳情查看。  
所有道具資料由 **JSON 配置檔驅動**，場景完全由程式碼建構。

**場景：** `Demo.unity`（Demo2 模組）  
**腳本路徑：** `Assets/Scripts/Demo2/`  
**資料路徑：** `Assets/StreamingAssets/ItemConfig.json`

---

## 系統架構圖

```
Demo2Setup (Composition Root)
│
├── ItemConfigLoader ─── 從 StreamingAssets/ItemConfig.json 載入道具定義
│
├── Camera Setup
│   └── Demo2CameraFollow ─── SmoothDamp 跟隨 + 預測位移
│
├── Physics World
│   └── Physics2D.gravity = (0, 0) ─── 零重力俯視角
│
├── Arena Generation
│   ├── Tiled Floor (Texture2D 程式化)
│   └── Box Walls (BoxCollider2D × 4)
│
├── Player
│   ├── Demo2PlayerController (WASD + 果凍效果)
│   └── PlayerStats (HP/MP 狀態管理)
│       ├── MaxHP: 100, MaxMP: 60
│       ├── ModifyHP / ModifyMP
│       ├── ApplyEffect(ItemEffect) ─── 策略分派
│       └── OnStatsChanged 事件
│
├── Collectibles (JSON 驅動生成)
│   ├── Collectible Component
│   │   ├── 漂浮 + 旋轉動畫
│   │   ├── OnTriggerEnter2D → 收集
│   │   ├── 傷害型道具 → 直接 ApplyEffect
│   │   └── 其他道具 → InventoryManager.AddItem
│   └── 4 種分類：Currency / Consumable / Quest / Collection
│
├── InventoryManager (Singleton + Observer)
│   ├── List<ItemSlot> ─── 格子式背包
│   ├── 可堆疊道具合併
│   ├── UseItem(slot) ─── 消耗品使用
│   ├── GetSlotsByCategory() ─── 分類篩選
│   └── OnInventoryChanged 事件
│
├── Demo2HUD (Observer)
│   ├── 分數 / 物品數
│   ├── HP 條 + MP 條 (Image.fillAmount)
│   └── 分類計數摘要
│
└── InventoryUI (B 鍵開關)
    ├── 分頁系統：全部 / 消耗品 / 任務 / 收藏品 / 貨幣
    ├── ScrollRect 道具列表
    ├── 詳情面板：名稱 / 圖示 / 說明 / 效果描述
    └── 使用按鈕（僅消耗品）
```

---

## 道具分類系統

| 分類 | 收集行為 | 背包操作 | 範例 |
|------|---------|---------|------|
| **Currency** | 碰觸收集 → 加分 + 進背包 | 查看 | 金幣、寶石、星星、愛心 |
| **Consumable** | 碰觸收集 → 進背包 | 點擊使用 (恢復 HP/MP) | 生命藥水、魔力藥水 |
| **Consumable (負面)** | 碰觸 → **即時扣 HP/MP** | 不進背包 | 毒霧瓶、魔力虹吸石 |
| **Quest** | 碰觸收集 → 進背包 | 查看說明 | 古老地圖、水晶鑰匙 |
| **Collection** | 碰觸收集 → 加分 + 進背包 | 查看說明 | 七彩羽毛、遠古化石 |

---

## 設計模式

| 模式 | 應用位置 | 說明 |
|------|---------|------|
| **Composition Root** | `Demo2Setup` | 完全程式碼建構場景 |
| **Singleton** | Manager 類 | 靜態 Instance 存取 |
| **Observer** | `OnInventoryChanged`, `OnStatsChanged` | 背包/狀態變更時通知 UI |
| **Data-Driven** | `ItemConfig.json` → `ItemConfigLoader` | 道具定義由外部 JSON 驅動 |
| **Strategy** | `PlayerStats.ApplyEffect()` | 根據 effect.type 分派不同行為 |
| **Slot-Based Inventory** | `InventoryManager.ItemSlot` | 格子式背包，可堆疊道具合併 |

---

## 技術亮點

### 1. JSON 驅動道具系統
```json
{
  "id": "potion_hp_small",
  "name": "小型生命藥水",
  "category": "Consumable",
  "description": "恢復 20 點 HP 的藥水。",
  "effect": { "type": "RestoreHP", "value": 20 }
}
```
- 所有道具定義在 `ItemConfig.json`
- 新增道具只需編輯 JSON，不需改程式碼
- 支援 Excel → JSON 轉換工具

### 2. HP/MP 狀態系統
- `PlayerStats` 管理 HP (100) / MP (60)
- `ApplyEffect()` 支援四種效果：RestoreHP、RestoreMP、DamageHP、DamageMP
- HP/MP 上限保護（`Mathf.Clamp`）
- 恢復型道具在已滿時無法使用

### 3. 完整背包 UI
- **B 鍵** 開關全螢幕背包面板
- **5 個分頁**：全部 / 消耗品 / 任務 / 收藏品 / 貨幣
- **ScrollRect** 滾動列表
- **詳情面板**：道具圖示、名稱、說明文字、效果描述
- **使用按鈕**：僅消耗品可點擊，即時恢復 HP/MP
- 所有 UI 純程式碼建構

### 4. 即時效果 vs 背包道具
```csharp
// Collectible.cs 中的收集邏輯
bool instantEffect = Entry.effect != null &&
    (Entry.effect.type == "DamageHP" || Entry.effect.type == "DamageMP");

if (instantEffect)
    PlayerStats.Instance.ApplyEffect(Entry.effect);  // 直接生效
else
    InventoryManager.Instance.AddItem(Entry);          // 進入背包
```

### 5. 程式化場景建構
- 地板磁磚、牆壁、玩家、所有道具完全程式碼生成
- 道具外觀（形狀、顏色、大小）由 JSON 定義
- 支援 circle / diamond / square 三種基礎圖形

---

## 資料流

```
StreamingAssets/ItemConfig.json
       │
       ▼
 ItemConfigLoader (UnityWebRequest, WebGL 相容)
       │
       ▼
 Demo2Setup.SpawnItems() ─── 根據 JSON 生成道具 GameObject
       │
       ▼
 Collectible (碰觸收集)
       │
  ┌────┴─────────────┐
  ▼                  ▼
即時效果            背包收集
(DamageHP/MP)      (其他道具)
  │                  │
  ▼                  ▼
PlayerStats      InventoryManager
  │                  │
  ▼                  ▼
OnStatsChanged    OnInventoryChanged
  │                  │
  ▼                  ▼
Demo2HUD          Demo2HUD + InventoryUI
(HP/MP 條)        (分數/道具/分頁)
```

---

## 檔案清單

### 核心
| 檔案 | 職責 |
|------|------|
| `Demo2Setup.cs` | 場景初始化 — 載入 JSON、建構場景 |
| `Demo2PlayerController.cs` | 玩家移動 + 視覺回饋 |
| `Demo2CameraFollow.cs` | SmoothDamp 相機跟隨 |

### 資料層
| 檔案 | 職責 |
|------|------|
| `Data/ItemData.cs` | 道具資料結構定義（ItemEntry, ItemEffect 等） |
| `Data/ItemConfigLoader.cs` | JSON 載入 + 快取（WebGL 相容） |

### 系統
| 檔案 | 職責 |
|------|------|
| `Collectible.cs` | 可收集物品元件（JSON 驅動） |
| `InventoryManager.cs` | 背包管理（格子、堆疊、使用） |
| `PlayerStats.cs` | HP/MP 狀態管理 + 效果系統 |

### UI
| 檔案 | 職責 |
|------|------|
| `Demo2HUD.cs` | HUD — 分數 / HP 條 / MP 條 |
| `InventoryUI.cs` | 背包面板（分頁 / 列表 / 詳情 / 使用） |

---

## Tools — 道具表格轉換

| 工具 | 說明 |
|------|------|
| `Tools/generate_items_from_excel.py` | Excel → ItemConfig.json 轉換 |

### Excel 格式（Sheet 名稱：「道具」）

| 欄 | 欄位名 | 說明 | 範例 |
|----|--------|------|------|
| A | id | 道具 ID | `potion_hp_small` |
| B | name | 道具名稱 | `小型生命藥水` |
| C | category | 分類 | `Currency` / `Consumable` / `Quest` / `Collection` |
| D | description | 說明文字 | `恢復 20 點 HP 的藥水。` |
| E | scoreValue | 分數價值 | `10` |
| F | stackable | 可否堆疊 | `TRUE` / `FALSE` |
| G | maxStack | 最大堆疊數 | `99` |
| H | spriteName | 圖形名稱 | `circle` / `diamond` / `square` |
| I | color_r | 顏色 R (0~1) | `1.0` |
| J | color_g | 顏色 G (0~1) | `0.85` |
| K | color_b | 顏色 B (0~1) | `0.2` |
| L | color_a | 顏色 A (0~1) | `1.0` |
| M | spriteScale | 圖形縮放 | `0.35` |
| N | effect_type | 效果類型 | `RestoreHP` / `RestoreMP` / `DamageHP` / `DamageMP` / 留空 |
| O | effect_value | 效果數值 | `20` / 留空 |

### 使用方式
```bash
pip install openpyxl
python Tools/generate_items_from_excel.py items.xlsx Assets/StreamingAssets/ItemConfig.json
```

---

## Unity API 使用

| API | 用途 |
|-----|------|
| `Rigidbody2D.MovePosition()` | 物理移動 |
| `CircleCollider2D` (isTrigger) | 觸發收集偵測 |
| `Image.fillAmount` | HP/MP 條填充 |
| `ScrollRect` + `VerticalLayoutGroup` | 背包捲動列表 |
| `UnityWebRequest` | WebGL 相容 JSON 載入 |
| `Input System` | WASD + B 鍵輸入 |
