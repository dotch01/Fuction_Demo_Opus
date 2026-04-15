using UnityEngine;

// ============================================================
// VoronoiExhibit.cs — Voronoi 圖
// 即時計算最近種子點 → 彩色區域
// ============================================================

public class VoronoiExhibit : ExhibitBase
{
    private Vector3[] seeds;
    private Color[] seedColors;
    private int seedCount = 8;
    private GameObject[,] pixels;
    private Renderer[,] pixelRenderers;
    private int res = 24; // 解析度
    private float gridSize = 4f;
    private TextMesh infoLabel;
    private float animTime;

    public override void BuildExhibit()
    {
        exhibitName = "Voronoi 圖";
        description = "空間中放置種子點\n每個像素找最近的種子\n→ 形成自然的區域分割！\n\n🎮 遊戲應用：\n• 地圖區域劃分（生態群落/勢力範圍）\n• 破碎效果（玻璃碎裂、地面龜裂）\n• 程序化城市街區佈局\n• 細胞/有機體紋理生成\n\n種子點緩慢漂移，觀察區域變化";
        formula = "V(sᵢ) = {x | d(x,sᵢ) < d(x,sⱼ) ∀j≠i}";
        challengeDescription = "觀看 Voronoi 動態變化 5 秒";

        seeds = new Vector3[seedCount];
        seedColors = new Color[seedCount];
        var rng = new System.Random(42);
        for (int i = 0; i < seedCount; i++)
        {
            seeds[i] = new Vector3((float)(rng.NextDouble() * 2 - 1) * 1.5f, (float)(rng.NextDouble() * 2 - 1) * 1.5f, 0);
            seedColors[i] = Color.HSVToRGB((float)i / seedCount, 0.7f, 0.85f);
        }

        pixels = new GameObject[res, res];
        pixelRenderers = new Renderer[res, res];
        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        float cellSize = gridSize / res;
        float offset = -gridSize / 2f;

        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = $"Vox_{x}_{y}";
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(offset + x * cellSize + cellSize * 0.5f, offset + y * cellSize + cellSize * 0.5f, 0);
                go.transform.localScale = new Vector3(cellSize * 0.95f, cellSize * 0.95f, 0.05f);

                var r = go.GetComponent<Renderer>();
                r.material = new Material(mat);
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                var col = go.GetComponent<Collider>();
                if (col != null) Object.Destroy(col);

                pixels[x, y] = go;
                pixelRenderers[x, y] = r;
            }
        }

        infoLabel = CreateLabel(new Vector3(0, -2.5f, 0), "", 24, new Color(0.7f, 0.7f, 0.8f));
    }

    public override void UpdateVisualization()
    {
        animTime += Time.deltaTime;

        // 種子漂移
        for (int i = 0; i < seedCount; i++)
        {
            float phase = i * 1.3f;
            seeds[i].x = Mathf.Sin(animTime * 0.3f + phase) * 1.5f;
            seeds[i].y = Mathf.Cos(animTime * 0.4f + phase * 0.7f) * 1.5f;
        }

        float cellSize = gridSize / res;
        float offset = -gridSize / 2f;

        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                Vector3 pos = new Vector3(offset + x * cellSize + cellSize * 0.5f, offset + y * cellSize + cellSize * 0.5f, 0);
                float minDist = float.MaxValue;
                int closest = 0;

                for (int s = 0; s < seedCount; s++)
                {
                    float d = Vector3.Distance(pos, seeds[s]);
                    if (d < minDist) { minDist = d; closest = s; }
                }

                // 邊界效果（距離第二近的種子很接近 → 暗化）
                float secondMin = float.MaxValue;
                for (int s = 0; s < seedCount; s++)
                {
                    if (s == closest) continue;
                    float d = Vector3.Distance(pos, seeds[s]);
                    if (d < secondMin) secondMin = d;
                }

                float edgeFactor = Mathf.Clamp01((secondMin - minDist) * 5f);
                pixelRenderers[x, y].material.color = seedColors[closest] * (0.3f + 0.7f * edgeFactor);
            }
        }

        // 畫種子點
        var mr = MathLineRenderer.Instance;
        if (mr != null)
        {
            for (int i = 0; i < seedCount; i++)
            {
                Vector3 sw = transform.TransformPoint(seeds[i]);
                mr.DrawLine(sw - Vector3.right * 0.04f, sw + Vector3.right * 0.04f, Color.white, 0.06f);
            }
        }

        infoLabel.text = $"{seedCount} 個種子點 · {res}×{res} 網格 · 邊界 = 等距線";
    }

    public override bool CheckChallengeComplete() => animTime > 5f;
}
