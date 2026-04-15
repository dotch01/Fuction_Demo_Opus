using UnityEngine;

// ============================================================
// ECSEntityListExhibit.cs — ECS 實體清單
// AoS vs SoA 記憶體佈局視覺化
// ============================================================

public class ECSEntityListExhibit : ExhibitBase
{
    private DragHandle handleMode; // 左=AoS 右=SoA
    private GameObject[,] memBlocks; // 記憶體方塊 8行×16列
    private Renderer[,] blockRenderers;
    private TextMesh modeLabel;
    private TextMesh cacheLabel;
    private TextMesh layoutLabel;

    private int rows = 6;     // 實體數
    private int cols = 12;    // 記憶體欄位
    private float blockSize = 0.3f;
    private bool isSoA = false;
    private int highlightComponent = 0; // 0=Position, 1=Velocity, 2=Health
    private float animTime;

    // 元件顏色
    private static readonly Color posColor = new Color(0.3f, 0.7f, 1f);
    private static readonly Color velColor = new Color(1f, 0.5f, 0.3f);
    private static readonly Color hpColor  = new Color(0.3f, 0.9f, 0.4f);
    private static readonly Color padColor = new Color(0.2f, 0.2f, 0.25f);

    public override void BuildExhibit()
    {
        exhibitName = "ECS 實體清單 Entity List";
        description = "遊戲引擎常用兩種記憶體佈局：\n\n• AoS (Array of Structs)：\n  [Pos,Vel,HP] [Pos,Vel,HP] ...\n  直覺，但遍歷單一元件 cache miss 多\n\n• SoA (Struct of Arrays)：\n  [Pos,Pos...] [Vel,Vel...]\n  同類元件連續，cache 友善！\n\n🎮 遊戲應用：\n• Unity DOTS/ECS 用 SoA 架構\n• 大量 Entity 效能可提升 10-100 倍\n• 子彈幕遊戲適合 SoA → 批次更新位置\n\n拖曳切換，按 E 高亮存取模式";
        formula = "SoA: Position[ ] → cache line 連續命中\nAoS: Entity[ ].Position → cache line 跳躍";
        challengeDescription = "切換到 SoA 模式，觀察 Position 連續存取";

        memBlocks = new GameObject[rows, cols];
        blockRenderers = new Renderer[rows, cols];

        float startX = -(cols * blockSize) / 2f;
        float startY = 1.5f;

        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = $"Mem_{r}_{c}";
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(startX + c * (blockSize + 0.02f), startY - r * (blockSize + 0.02f), 0);
                go.transform.localScale = Vector3.one * blockSize * 0.95f;

                var rend = go.GetComponent<Renderer>();
                rend.material = new Material(mat);
                rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                var col = go.GetComponent<Collider>();
                if (col != null) Object.Destroy(col);

                memBlocks[r, c] = go;
                blockRenderers[r, c] = rend;
            }
        }

        handleMode = CreateDragHandle(new Vector3(0, -2f, 0), new Color(0.8f, 0.8f, 0.3f), 0.15f);
        handleMode.minBounds = new Vector3(-2f, -2f, 0);
        handleMode.maxBounds = new Vector3(2f, -2f, 0);

        modeLabel = CreateLabel(new Vector3(0, -2.7f, 0), "", 36, Color.white);
        cacheLabel = CreateLabel(new Vector3(0, -3.4f, 0), "", 28, new Color(0.7f, 0.8f, 1f));
        layoutLabel = CreateLabel(new Vector3(0, 2.3f, 0), "", 24, new Color(0.6f, 0.6f, 0.7f));

        CreateLabel(new Vector3(-2.5f, -2f, 0), "AoS", 24, new Color(0.5f, 0.5f, 0.6f));
        CreateLabel(new Vector3(2.5f, -2f, 0), "SoA", 24, new Color(0.5f, 0.5f, 0.6f));
    }

    public override void UpdateVisualization()
    {
        isSoA = handleMode.LocalPosition.x > 0;
        animTime += Time.deltaTime * 3f;
        int scanIdx = (int)(animTime % (rows * 3)) ; // 掃描索引

        if (isSoA)
        {
            // SoA: 每 2 列一組元件（Pos, Pos, ..., Vel, Vel, ..., HP, HP, ...）
            layoutLabel.text = "SoA: [Pos₁ Pos₂ Pos₃ ...] [Vel₁ Vel₂ Vel₃ ...] [HP₁ HP₂ HP₃ ...]";
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int compGroup = c / 4; // 0=Pos, 1=Vel, 2=HP
                    Color baseC = compGroup == 0 ? posColor : compGroup == 1 ? velColor : hpColor;

                    // 高亮目前掃描到的元件
                    bool isScanning = (compGroup == highlightComponent) && (r * 4 + (c % 4)) == scanIdx;
                    blockRenderers[r, c].material.color = isScanning ? Color.white : baseC;
                }
            }
            int cacheMiss = rows; // SoA 連續存取，miss 少
            cacheLabel.text = $"存取 Position：連續存取 → 約 {cacheMiss} 次 cache line 載入 ✓";
        }
        else
        {
            // AoS: 每列一個實體 (Pos,Vel,HP,pad,Pos,Vel,HP,pad...)
            layoutLabel.text = "AoS: [Pos₁ Vel₁ HP₁ pad] [Pos₂ Vel₂ HP₂ pad] ...";
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int compInEntity = c % 4; // 0=Pos, 1=Vel, 2=HP, 3=pad
                    Color baseC;
                    if (compInEntity == 0) baseC = posColor;
                    else if (compInEntity == 1) baseC = velColor;
                    else if (compInEntity == 2) baseC = hpColor;
                    else baseC = padColor;

                    int entityIdx = r * 3 + c / 4;
                    bool isScanning = (compInEntity == highlightComponent) && entityIdx == scanIdx;
                    blockRenderers[r, c].material.color = isScanning ? Color.white : baseC;
                }
            }
            int cacheMiss = rows * 3; // AoS 跳躍存取
            cacheLabel.text = $"存取 Position：跳躍存取 → 約 {cacheMiss} 次 cache line 載入 ✗";
        }

        modeLabel.text = isSoA ? "模式：SoA (Struct of Arrays)" : "模式：AoS (Array of Structs)";
        modeLabel.color = isSoA ? new Color(0.3f, 1f, 0.5f) : new Color(1f, 0.7f, 0.3f);
    }

    protected override void OnChallengeStart()
    {
        highlightComponent = (highlightComponent + 1) % 3;
    }

    public override bool CheckChallengeComplete()
    {
        return isSoA && highlightComponent == 0;
    }

    protected override void OnChallengeCompleted()
    {
        base.OnChallengeCompleted();
        ChallengeSystem.Instance?.MarkCompleted(exhibitName);
    }
}
