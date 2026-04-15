# 03 — Math Museum Demo（互動式數學博物館）

## 概述

3D 第一人稱互動數學博物館，涵蓋 **10 個展區、50+ 展品**，每個展品視覺化一個數學/計算機科學概念。  
玩家可透過 **拖曳控制點** 即時互動，完成挑戰任務。  
全場景由程式碼建構，包含自訂 Shader、物件池化線段渲染器、挑戰追蹤系統。

**場景：** `GameMath.unity`  
**腳本路徑：** `Assets/Scripts/MathDemo/`

---

## 展區配置

| 區域 | 展品數 | 主題 |
|------|--------|------|
| **Vector Hall** | 8 | 向量運算：方向、距離、內積、外積、投影、反射、法線、圓上最近點 |
| **Rotation Wing** | 7 | 旋轉：尤拉角、軸角、四元數、Slerp、齊次矩陣、父子座標、座標轉換 |
| **Curve Corridor** | 6 | 曲線：Lerp/Remap、三次曲線對比、三次樣條、Hermite、Easing、SmoothDamp |
| **Trig Tower** | 5 | 三角函數：單位圓、Sin/Cos/Tan 波形、Atan2、複數乘法、歐拉恆等式 |
| **Collision Arena** | 7 | 碰撞偵測：AABB、三角形內點、線段交叉、射線三角形、多重反射、牛頓根 |
| **Physics Lab** | 5 | 物理：速度積分、彈簧、最大彈跳高度、積分距離、焦散 |
| **Render Gallery** | 7 | 渲染：Billboard、LookAt、視錐剔除、透視矩陣、渲染管線、三角面、VBO |
| **Calculus Garden** | 4 | 微積分：導數、積分面積、辛普森法則、三角剖分 |
| **Procedural Forge** | 4 | 程序生成：碎形、L-System、Voronoi、波疊加 |
| **Algorithm Hall** | 9 | 演算法：BFS、Dijkstra、A*、二分搜尋、合併排序、堆、路徑比較、ECS、樹 |

---

## 系統架構圖

```
MathMuseumManager (Composition Root)
│
├── Zone Layout System
│   ├── 10 個 Zone (20m × 20m 每區)
│   ├── GridFloor.shader 無限網格地板
│   ├── 牆壁 + 天花板
│   └── 點光源照明
│
├── PlayerController (FPS)
│   ├── CharacterController
│   ├── WASD + 右鍵滑鼠視角
│   ├── 重力 + 跳躍
│   └── WebGL 友善（游標始終可見）
│
├── ExhibitBase (Template Method 抽象基類)
│   ├── BuildExhibit() ─── 建構展品視覺元素
│   ├── UpdateVisualization() ─── 每幀更新
│   └── CheckChallengeComplete() ─── 驗證挑戰條件
│
├── DragHandle System
│   ├── 3D 可拖曳控制點
│   ├── 滑鼠射線 → 平面投影
│   ├── XY / XZ / Free 拖曳模式
│   └── onPositionChanged 回呼
│
├── ExhibitTrigger
│   ├── OnTriggerEnter → 啟動展品
│   ├── OnTriggerExit → 關閉展品
│   └── 觸發 InfoPanel 顯示
│
├── ChallengeSystem (Singleton + Observer)
│   ├── 全域挑戰追蹤
│   ├── RegisterExhibit / MarkComplete
│   └── onChallengeCompleted 事件
│
├── MathLineRenderer (Singleton + Object Pool)
│   ├── Immediate-Mode 繪圖 API
│   │   ├── DrawLine / DrawArrow
│   │   ├── DrawCircle / DrawArc
│   │   ├── DrawDashedLine
│   │   ├── DrawPoint / DrawTriangle
│   │   └── DrawAABB
│   └── LineRenderer 物件池（每幀回收重用）
│
└── UI Layer
    ├── ExhibitInfoPanel ─── 左側資訊面板（名稱/描述/公式/挑戰狀態）
    ├── ChallengeHUD ─── 右上角進度 + 完成通知
    └── InteractionPrompt ─── "按 E 互動" 提示
```

---

## 設計模式

| 模式 | 應用位置 | 說明 |
|------|---------|------|
| **Composition Root** | `MathMuseumManager` | 建構 10 個區域、50+ 展品、Player、UI |
| **Template Method** | `ExhibitBase` | 抽象 `Build` / `Update` / `CheckChallenge`，子類實作 |
| **Abstract Factory** | `ExhibitBase` helper methods | `CreatePrimitive`, `CreateLabel`, `CreateDragHandle` |
| **Object Pool** | `MathLineRenderer` | LineRenderer 每幀回收重用，避免頻繁 GC |
| **Singleton** | Manager 類 | 全域存取 |
| **Observer** | `ChallengeSystem` | 挑戰完成事件通知 UI |
| **Immediate Mode API** | `MathLineRenderer` | 類似 Debug.DrawLine 的 API，但用於 Runtime |

---

## 技術亮點

### 1. Immediate-Mode 線段渲染系統
```csharp
// 每幀使用，無需管理生命週期
MathLineRenderer.Instance.DrawArrow(origin, target, Color.red);
MathLineRenderer.Instance.DrawCircle(center, radius, Color.blue);
MathLineRenderer.Instance.DrawArc(center, from, to, Color.green, segments);
```
- 池化 `LineRenderer` 元件，每幀標記重置 → 使用 → 隱藏剩餘
- 支援箭頭、虛線、圓弧、三角形、AABB 等多種圖元
- 效能優化：避免每幀 `Instantiate` / `Destroy`

### 2. 展品架構（Template Method）
每個展品繼承 `ExhibitBase`：
```csharp
class DotProductExhibit : ExhibitBase
{
    override void BuildExhibit() { /* 建構拖曳把手、標籤 */ }
    override void UpdateVisualization() { /* 繪製向量、角度弧、數值 */ }
    override void CheckChallengeComplete() { /* 內積 ≈ 0 → 完成 */ }
}
```
- 統一的生命週期管理
- `DragHandle` 提供即時互動
- 每個展品對應一個數學公式 + 視覺化 + 挑戰條件

### 3. 自訂 Shader

| Shader | 技術 |
|--------|------|
| `GridFloor.shader` | 程序化無限網格、主次線、軸色、距離淡出 |
| `TransparentColor.shader` | 簡單透明色 |
| `ZoneHighlight.shader` | 區域邊界高光 |

### 4. 3D 拖曳系統（DragHandle）
- 滑鼠射線投射到指定平面（XY / XZ / 自由）
- 懸停高光回饋
- `onPositionChanged` 回呼驅動展品更新
- 支援多把手同時存在

### 5. 50+ 數學概念完整實作
每個展品都包含：
- 數學公式的即時視覺化
- 可拖曳互動控制點
- 挑戰目標（如：讓兩向量垂直、找到最短路徑等）
- 說明面板文字

---

## 展品實作範例

### Vector Hall — Dot Product Exhibit
```
BuildExhibit():
  - 創建兩個 DragHandle (A, B 向量端點)
  - 創建標籤顯示內積值

UpdateVisualization():
  - DrawArrow(origin → A, red)
  - DrawArrow(origin → B, blue)
  - DrawArc(angle between A and B)
  - DrawDashedLine(projection of A onto B)
  - 更新標籤: "A·B = |A||B|cos(θ) = {value}"

CheckChallengeComplete():
  - 判定 |A·B| < 0.05 (接近垂直)
```

### Algorithm Hall — A* Exhibit
```
BuildExhibit():
  - 創建 Grid (tilemap)
  - 創建 Start/End DragHandle
  - 創建障礙物

UpdateVisualization():
  - 執行 A* 演算法
  - 著色 Open/Closed 節點
  - DrawLine 最短路徑
  - 顯示 f(n) = g(n) + h(n) 值

CheckChallengeComplete():
  - 路徑找到且長度 ≤ 最佳解 +1
```

---

## 檔案清單

### Core
| 檔案 | 職責 |
|------|------|
| `Core/MathMuseumManager.cs` | 場景初始化 — 建構所有區域與展品 |
| `Core/ExhibitBase.cs` | 展品抽象基類 |
| `Core/ExhibitTrigger.cs` | 接近觸發啟動/關閉展品 |
| `Core/ChallengeSystem.cs` | 全域挑戰追蹤 |
| `Core/PlayerController.cs` | 第一人稱控制器 |
| `Core/DragHandle.cs` | 3D 拖曳控制點 |

### Rendering
| 檔案 | 職責 |
|------|------|
| `Rendering/MathLineRenderer.cs` | 物件池化線段渲染器 |
| `Rendering/Shaders/GridFloor.shader` | 無限網格地板 |
| `Rendering/Shaders/TransparentColor.shader` | 透明色 |
| `Rendering/Shaders/ZoneHighlight.shader` | 區域高光 |

### UI
| 檔案 | 職責 |
|------|------|
| `UI/ExhibitInfoPanel.cs` | 展品資訊面板 |
| `UI/ChallengeHUD.cs` | 挑戰進度 HUD |
| `UI/InteractionPrompt.cs` | 互動提示 |

### Exhibits（50+ 檔案）
| 展區 | 檔案範例 |
|------|---------|
| VectorHall | `DirectionExhibit.cs`, `DotProductExhibit.cs`, `CrossProductExhibit.cs` 等 |
| RotationWing | `QuaternionExhibit.cs`, `SlerpExhibit.cs`, `HomogeneousMatrixExhibit.cs` 等 |
| CurveCorridor | `CubicSplineExhibit.cs`, `EasingExhibit.cs`, `SmoothDampExhibit.cs` 等 |
| TrigTower | `UnitCircleExhibit.cs`, `Atan2Exhibit.cs`, `EulerIdentityExhibit.cs` 等 |
| CollisionArena | `AABBExhibit.cs`, `RayTriangleExhibit.cs`, `NewtonRootExhibit.cs` 等 |
| PhysicsLab | `VelocityExhibit.cs`, `SpringExhibit.cs`, `CausticsExhibit.cs` 等 |
| RenderGallery | `FrustumCullingExhibit.cs`, `PerspectiveMatrixExhibit.cs`, `VBOExhibit.cs` 等 |
| CalculusGarden | `DerivativeExhibit.cs`, `SimpsonExhibit.cs`, `TriangulationExhibit.cs` 等 |
| ProceduralForge | `FractalExhibit.cs`, `LSystemExhibit.cs`, `VoronoiExhibit.cs` 等 |
| AlgorithmHall | `BFSExhibit.cs`, `AStarExhibit.cs`, `HeapExhibit.cs` 等 |

---

## Tools

本 Demo 不使用外部資料檔案，所有展品定義直接寫在 `MathMuseumManager.cs` 中。  
若未來需要將展品配置外部化，可參考 Demo1 的 `DataLoader` 模式建立 `ExhibitConfig.json`。
