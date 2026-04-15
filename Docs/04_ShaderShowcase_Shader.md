# 04 — Shader Showcase（Shader 互動展示）

## 概述

互動式 Shader 技術展示畫廊，包含 **26 種 HLSL Shader**，每個 Shader 配有 3D 展示場景與即時參數滑桿。  
支援鍵盤 (A/D) 或按鈕切換展品，可即時調整 Shader 參數觀察效果變化。

**場景：** `Shader.unity`  
**腳本路徑：** `Assets/Scripts/Shader/`  
**Shader 路徑：** `Assets/Scripts/Shader/Shaders/`

---

## 系統架構圖

```
ShowcaseManager (Singleton, Scene Controller)
│
├── Camera (URP, Orthographic/Perspective)
├── Directional Light + Ambient
│
├── ShowcaseSetup (Static Factory)
│   └── 為每個 Shader 建構 3D 展示物件
│       ├── Primitive 排列（球/方塊/柱/面片）
│       ├── 動態建立 Material
│       └── SliderParam[] 參數定義
│
├── Navigation System
│   ├── A / D 鍵 或 ← → 箭頭
│   ├── UI 按鈕 (< / >)
│   └── 展品名稱顯示
│
├── Parameter Sliders
│   ├── 每個 Shader 有獨立 SliderParam 組
│   ├── 即時更新 Material.SetFloat / SetColor
│   └── 動態建構 Slider UI
│
└── PortalRenderer (特殊展品)
    ├── 第二相機 + RenderTexture
    ├── Layer 31 隔離「異世界」場景
    └── 輸出至 Portal.shader 採樣
```

---

## Shader 技術清單

### 光照與材質

| Shader | 技術 | 核心演算法 |
|--------|------|-----------|
| `Fresnel.shader` | 菲涅爾效應 | `pow(1 - NdotV, power)` |
| `RimLight.shader` | 邊緣光 | 反向菲涅爾 + 自訂色 |
| `NormalMap.shader` | 法線貼圖 | 切線空間法線映射 |
| `SSS.shader` | 次表面散射 | Half-Lambert + 厚度近似 |

### 噪聲

| Shader | 技術 | 核心演算法 |
|--------|------|-----------|
| `HashNoise.shader` | 雜湊噪聲 | `frac(sin(dot(uv, vec)) * 43758.5453)` |
| `PerlinNoise.shader` | 柏林噪聲 | 梯度插值 + smoothstep |
| `VoronoiNoise.shader` | Voronoi 噪聲 | 最近種子點距離 |

### 自然效果

| Shader | 技術 | 核心演算法 |
|--------|------|-----------|
| `Water.shader` | 水面 | 多層 sin 波頂點偏移 + 菲涅爾 + UV 動畫 |
| `Fire.shader` | 火焰 | FBM 噪聲 + UV 捲動 + 色彩漸層 |
| `VolumetricCloud.shader` | 體積雲 | **光線步進 (Ray Marching)** + 3D FBM + Beer-Lambert 吸收 |
| `Lightning.shader` | 閃電 | 1D 噪聲路徑 + 分支 + exp() 光暈 |
| `Fog.shader` | 霧 | 深度相關的指數衰減 |
| `Rain.shader` | 雨 | 程序化雨滴 + 漣漪 |
| `Bubble.shader` | 泡沫 | 菲涅爾 + 薄膜干涉彩虹 |

### 幾何效果

| Shader | 技術 | 核心演算法 |
|--------|------|-----------|
| `Outline.shader` | 描邊 | 雙 Pass：Cull Front 法線外擴 + 正常渲染 |
| `ClothFlag.shader` | 布料旗幟 | 頂點 Shader 風力模擬 + 法線重算 |
| `POM.shader` | 視差遮蔽映射 | 光線步進 + 二分精煉 |

### 特效

| Shader | 技術 | 核心演算法 |
|--------|------|-----------|
| `Portal.shader` | 傳送門 | Stencil Buffer + RenderTexture + 邊緣光暈 |
| `SSR.shader` | 螢幕空間反射 | 螢幕空間光線追蹤 |
| `Glow.shader` | 發光 | Additive Blend |
| `Particles.shader` | 粒子 | Additive + 軟粒子 |

### 後處理

| Shader | 技術 | 核心演算法 |
|--------|------|-----------|
| `ChromaticAberration.shader` | 色散 | RGB 通道偏移 |
| `ColorGrading.shader` | 色彩分級 | 亮度/對比/飽和度調整 |
| `Dithering.shader` | 抖動 | Bayer 矩陣 |

### Stencil Buffer

| Shader | 技術 | 核心演算法 |
|--------|------|-----------|
| `StencilWrite.shader` | Stencil 寫入 | 寫入 Stencil 值，不渲染像素 |
| `StencilRead.shader` | Stencil 讀取 | 條件渲染（遮罩效果） |

---

## 設計模式

| 模式 | 應用位置 | 說明 |
|------|---------|------|
| **Singleton** | `ShowcaseManager` | 場景管理器 |
| **Static Factory** | `ShowcaseSetup` | 為每個 Shader 建構專屬 3D 展示場景 |
| **Data-Driven** | `SliderParam` 結構 | 每個 Shader 的可調參數由結構體定義 |
| **Second Camera** | `PortalRenderer` | RenderTexture 技術實現傳送門 |

---

## 技術亮點

### 1. 光線步進體積雲（VolumetricCloud.shader）
```hlsl
for (int i = 0; i < MAX_STEPS; i++) {
    float density = fbm3D(pos * _NoiseScale + _Time.y * _WindSpeed);
    if (density > _DensityThreshold) {
        float transmittance = exp(-density * _Absorption * stepSize); // Beer-Lambert
        color += density * _CloudColor * transmittance;
        totalTransmittance *= transmittance;
    }
    pos += rayDir * stepSize;
}
```
- 3D FBM 噪聲生成雲密度場
- Beer-Lambert 定律計算光線吸收
- 支援風向、密度、吸收率即時調整

### 2. 傳送門效果（Portal System）
```
PortalRenderer
├── Camera (Layer 31 only)
│   └── RenderTexture (512×512)
├── 異世界場景物件 (Layer 31)
│   └── 行星 + 懸浮方塊 + 粒子
└── Portal.shader
    ├── Stencil Write (門框)
    └── Stencil Read (RenderTexture 顯示)
```

### 3. 視差遮蔽映射（POM.shader）
- 從相機方向步進 UV 空間
- 二分法精煉最終深度
- 配合法線貼圖實現深度錯覺

### 4. 所有 Shader 均為 URP 相容
```hlsl
Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
```

---

## 檔案清單

### Scripts
| 檔案 | 職責 |
|------|------|
| `Scripts/ShowcaseManager.cs` | 場景管理、導航、參數 UI |
| `Scripts/ShowcaseSetup.cs` | 靜態工廠：建構展品物件 |
| `Scripts/PortalRenderer.cs` | 傳送門第二相機管理 |

### Shaders（26 檔案）
| 檔案 | 類別 |
|------|------|
| `Fresnel.shader` | 光照 |
| `RimLight.shader` | 光照 |
| `NormalMap.shader` | 光照 |
| `SSS.shader` | 光照 |
| `HashNoise.shader` | 噪聲 |
| `PerlinNoise.shader` | 噪聲 |
| `VoronoiNoise.shader` | 噪聲 |
| `Water.shader` | 自然 |
| `Fire.shader` | 自然 |
| `VolumetricCloud.shader` | 自然 |
| `Lightning.shader` | 自然 |
| `Fog.shader` | 自然 |
| `Rain.shader` | 自然 |
| `Bubble.shader` | 自然 |
| `Outline.shader` | 幾何 |
| `ClothFlag.shader` | 幾何 |
| `POM.shader` | 幾何 |
| `Portal.shader` | 特效 |
| `SSR.shader` | 特效 |
| `Glow.shader` | 特效 |
| `Particles.shader` | 特效 |
| `ChromaticAberration.shader` | 後處理 |
| `ColorGrading.shader` | 後處理 |
| `Dithering.shader` | 後處理 |
| `StencilWrite.shader` | Stencil |
| `StencilRead.shader` | Stencil |

---

## Tools

本 Demo 不使用外部資料檔案，所有 Shader 展品由 `ShowcaseSetup.cs` 靜態工廠方法建構。  
Shader 參數透過 `SliderParam` 結構體直接定義在程式碼中。
