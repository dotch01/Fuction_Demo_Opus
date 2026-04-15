using UnityEngine;

// ============================================================
// VBOExhibit.cs — 頂點緩衝 VBO
// 視覺化 Vertex → VBO → GPU 流程
// ============================================================

public class VBOExhibit : ExhibitBase
{
    private TextMesh[] stageLabels = new TextMesh[4];
    private TextMesh infoLabel;
    private float animTime;

    private static readonly string[] stages = { "CPU 頂點", "VBO 上傳", "GPU 頂點著色器", "光柵化+片元" };
    private static readonly Color[] stageColors = {
        new Color(0.3f, 0.7f, 1f), new Color(0.5f, 0.9f, 0.4f),
        new Color(1f, 0.7f, 0.3f), new Color(1f, 0.4f, 0.5f)
    };

    public override void BuildExhibit()
    {
        exhibitName = "頂點緩衝 VBO";
        description = "Vertex Buffer Object — GPU 的資料容器\n\n流程：\n① CPU 準備頂點資料（位置、法線、UV）\n② 上傳到 VBO（GPU 記憶體）\n③ 頂點著色器處理每個頂點\n④ 光柵化 → 片元著色器\n\n🎮 遊戲應用：\n• 靜態合批 Static Batching 共享 VBO\n• 減少 Draw Call 的核心優化手法\n• GPU Instancing 一次繪製大量物件\n\n觀察資料流動動畫";
        formula = "glBufferData(GL_ARRAY_BUFFER, ...)";
        challengeDescription = "觀看完整一輪資料流動";

        float xStart = -2.5f, xStep = 1.7f;
        for (int i = 0; i < 4; i++)
        {
            float x = xStart + i * xStep;
            CreateStaticPrimitive(PrimitiveType.Cube, new Vector3(x, 0, 0), new Vector3(1.2f, 0.8f, 0.15f), stageColors[i] * 0.4f);
            stageLabels[i] = CreateLabel(new Vector3(x, 0, 0), stages[i], 22, stageColors[i]);
        }

        infoLabel = CreateLabel(new Vector3(0, -2f, 0), "", 26, Color.white);
    }

    public override void UpdateVisualization()
    {
        var mr = MathLineRenderer.Instance;
        if (mr == null) return;

        animTime += Time.deltaTime;
        float cycle = animTime % 4f;
        int currentStage = Mathf.FloorToInt(cycle);

        float xStart = -2.5f, xStep = 1.7f;

        // 箭頭連接各階段
        for (int i = 0; i < 3; i++)
        {
            float x0 = xStart + i * xStep + 0.65f;
            float x1 = xStart + (i + 1) * xStep - 0.65f;
            Color c = i <= currentStage ? stageColors[i] : new Color(0.3f, 0.3f, 0.35f);
            mr.DrawArrow(transform.TransformPoint(new Vector3(x0, 0, 0)),
                         transform.TransformPoint(new Vector3(x1, 0, 0)),
                         c, 0.01f, 0.05f);
        }

        // 資料包動畫
        float dataX = Mathf.Lerp(xStart, xStart + 3 * xStep, cycle / 4f);
        mr.DrawLine(transform.TransformPoint(new Vector3(dataX - 0.05f, 0.8f, 0)),
                    transform.TransformPoint(new Vector3(dataX + 0.05f, 0.8f, 0)),
                    Color.yellow, 0.06f);

        string[] details = {
            "① CPU 建立頂點陣列：Position, Normal, UV, Color",
            "② glBufferData → 從 RAM 拷貝到 VRAM",
            "③ Vertex Shader：MVP 變換、骨骼動畫",
            "④ Rasterize → Fragment Shader → Framebuffer"
        };

        infoLabel.text = details[currentStage];
    }

    public override bool CheckChallengeComplete()
    {
        return animTime > 4f;
    }
}
