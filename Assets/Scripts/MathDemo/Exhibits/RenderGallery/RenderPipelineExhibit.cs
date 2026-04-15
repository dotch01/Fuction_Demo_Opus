using UnityEngine;

// ============================================================
// RenderPipelineExhibit.cs — 渲染管線
// 頂點 → 光柵化 → Fragment 逐步動畫
// ============================================================

public class RenderPipelineExhibit : ExhibitBase
{
    private TextMesh[] stageLabels = new TextMesh[5];
    private TextMesh detailLabel;
    private float animTime;

    private static readonly string[] stageNames = {
        "頂點輸入", "頂點著色器", "圖元裝配", "光柵化", "片元著色器"
    };
    private static readonly Color[] colors = {
        new Color(0.3f, 0.7f, 1f), new Color(0.5f, 0.9f, 0.4f),
        new Color(1f, 0.85f, 0.3f), new Color(1f, 0.5f, 0.3f),
        new Color(0.8f, 0.3f, 1f)
    };
    private static readonly string[] details = {
        "CPU 送出 Vertex Buffer：位置、法線、UV、顏色",
        "MVP 矩陣變換：Model→World→View→Clip Space",
        "三角形組裝 + 裁切 (Clipping) + 背面剔除",
        "三角形 → 像素覆蓋 → 插值頂點屬性（Barycentric）",
        "Texture Sampling + Lighting + Shadow → 最終像素顏色"
    };

    public override void BuildExhibit()
    {
        exhibitName = "渲染管線 Render Pipeline";
        description = "GPU 繪圖流水線：\n\n① 頂點輸入 → VBO 資料\n② 頂點著色器 → MVP 變換\n③ 圖元裝配 → 三角形\n④ 光柵化 → 像素\n⑤ 片元著色器 → 最終顏色\n\n🎮 遊戲應用：\n• 每個你看到的 3D 物件都經過此管線\n• Shader 就是控制 ② 和 ⑤ 的程式\n• 了解管線 = 寫好 Shader 的基礎\n\n觀看逐步動畫";
        formula = "V → VS → Assembly → Rasterize → FS → Pixel";
        challengeDescription = "觀看完整一輪管線";

        float yStart = 1.8f, yStep = -0.9f;
        for (int i = 0; i < 5; i++)
        {
            float y = yStart + i * yStep;
            CreateStaticPrimitive(PrimitiveType.Cube, new Vector3(0, y, 0), new Vector3(3f, 0.5f, 0.15f), colors[i] * 0.3f);
            stageLabels[i] = CreateLabel(new Vector3(0, y, 0), stageNames[i], 24, colors[i]);
        }

        detailLabel = CreateLabel(new Vector3(0, -2.8f, 0), "", 22, new Color(0.7f, 0.7f, 0.8f));
    }

    public override void UpdateVisualization()
    {
        var mr = MathLineRenderer.Instance;
        if (mr == null) return;

        animTime += Time.deltaTime;
        float cycle = animTime % 5f;
        int currentStage = Mathf.FloorToInt(cycle);

        float yStart = 1.8f, yStep = -0.9f;

        // 箭頭
        for (int i = 0; i < 4; i++)
        {
            float y0 = yStart + i * yStep - 0.3f;
            float y1 = yStart + (i + 1) * yStep + 0.3f;
            Color c = i <= currentStage ? colors[i] : new Color(0.3f, 0.3f, 0.35f);
            mr.DrawArrow(transform.TransformPoint(new Vector3(0, y0, 0)),
                         transform.TransformPoint(new Vector3(0, y1, 0)),
                         c, 0.01f, 0.05f);
        }

        // 高亮當前階段
        for (int i = 0; i < 5; i++)
        {
            stageLabels[i].color = i == currentStage ? Color.white : colors[i] * 0.7f;
        }

        // 資料流動
        float dataY = Mathf.Lerp(yStart, yStart + 4 * yStep, cycle / 5f);
        mr.DrawLine(transform.TransformPoint(new Vector3(-0.08f, dataY, 0)),
                    transform.TransformPoint(new Vector3(0.08f, dataY, 0)),
                    Color.yellow, 0.08f);

        detailLabel.text = details[currentStage];
    }

    public override bool CheckChallengeComplete()
    {
        return animTime > 5f;
    }
}
