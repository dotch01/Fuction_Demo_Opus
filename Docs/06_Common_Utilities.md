# 06 — Common Utilities（共用工具模組）

## 概述

所有 Demo 共用的工具程式碼，提供字型載入與教學提示兩個功能。

**腳本路徑：** `Assets/Scripts/Common/`

---

## 模組

### FontHelper（字型輔助工具）

| 項目 | 說明 |
|------|------|
| **檔案** | `FontHelper.cs` |
| **類型** | 靜態工具類 |
| **功能** | 從 Resources 載入 `NotoSansTC-Regular` 字型，失敗時回退至 Unity 內建字型 |
| **模式** | Lazy 快取（首次呼叫時載入，之後直接回傳快取） |

```csharp
// 使用方式
Font font = FontHelper.GetFont(); // 自動載入或回退
```

**Unity API：** `Resources.Load<Font>()`, `Resources.GetBuiltinResource<Font>()`

---

### TutorialHint（教學提示面板）

| 項目 | 說明 |
|------|------|
| **檔案** | `TutorialHint.cs` |
| **類型** | 靜態工廠 + MonoBehaviour |
| **功能** | 程式碼建構左上角半透明教學提示，15 秒自動淡出消失，附關閉按鈕 |
| **模式** | Static Factory (`Show()` 靜態方法建立實例) |

```csharp
// 使用方式
TutorialHint.Show("歡迎來到太空探索！\n使用 WASD 移動...");
```

**技術細節：**
- 程式碼建構 `Canvas` → `CanvasGroup` → `VerticalLayoutGroup`
- `ContentSizeFitter` 自動適配內容大小
- Coroutine 控制 15 秒 → 淡出動畫 → `Destroy()`
- 關閉按鈕提前觸發淡出

**Unity API：** `CanvasGroup`, `VerticalLayoutGroup`, `ContentSizeFitter`, `Coroutine`

---

## 設計理念

這兩個工具體現了整個專案的核心設計原則：

1. **Zero-Prefab** — 所有 UI 元素完全由程式碼建構，不依賴 Prefab
2. **Static API** — 提供簡潔的靜態方法呼叫介面
3. **自給自足** — 工具自行管理生命週期（載入、顯示、銷毀）
