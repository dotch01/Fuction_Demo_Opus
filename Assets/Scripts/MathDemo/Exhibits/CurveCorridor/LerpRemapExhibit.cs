using UnityEngine;

// ============================================================
// LerpRemapExhibit.cs — 線性插值 + Remap
// 拖 t 滑桿，球在 A-B 間移動 + 顯示 Remap 映射
// ============================================================

public class LerpRemapExhibit : ExhibitBase
{
    private DragHandle handleT;
    private GameObject lerpBall;
    private TextMesh lerpLabel;
    private TextMesh remapLabel;

    private Vector3 pointA = new Vector3(-3, 1, 0);
    private Vector3 pointB = new Vector3(3, 1, 0);

    public override void BuildExhibit()
    {
        exhibitName = "Lerp & Remap";
        description = "lerp(a, b, t) = a + (b-a) × t\n\n線性插值是最基本的動畫工具\nRemap 將一個範圍映射到另一個範圍\n\n🎮 遊戲應用：\n• HP 條：Lerp(紅, 綠, hp/maxHp)\n• 傷害數字淡出：alpha = Lerp(1, 0, t)\n• 搖桿 Remap：[-1,1] → [minSpeed, maxSpeed]\n• Color.Lerp 做漸層過渡\n\n拖曳觀察 Lerp 和 Remap 結果";
        formula = "lerp(a,b,t) = a(1-t) + bt\nremap(v, iMin, iMax, oMin, oMax)";
        challengeDescription = "讓 t 精準停在 0.75（±0.02）";

        // A、B 點
        CreateStaticPrimitive(PrimitiveType.Sphere, pointA, Vector3.one * 0.2f, new Color(1f, 0.3f, 0.3f));
        CreateStaticPrimitive(PrimitiveType.Sphere, pointB, Vector3.one * 0.2f, new Color(0.3f, 0.5f, 1f));
        CreateLabel(pointA + Vector3.up * 0.4f, "A", 28, new Color(1f, 0.4f, 0.4f));
        CreateLabel(pointB + Vector3.up * 0.4f, "B", 28, new Color(0.4f, 0.5f, 1f));

        // 移動中的球
        lerpBall = CreateStaticPrimitive(PrimitiveType.Sphere, pointA, Vector3.one * 0.35f, new Color(1f, 0.9f, 0.3f));

        // t 滑桿
        handleT = CreateDragHandle(new Vector3(-3, -1.5f, 0), new Color(1f, 0.9f, 0.3f), 0.13f);
        handleT.minBounds = new Vector3(-3f, -1.5f, 0);
        handleT.maxBounds = new Vector3(3f, -1.5f, 0);

        CreateLabel(new Vector3(-3.2f, -1.1f, 0), "t=0", 22, new Color(0.5f, 0.5f, 0.5f));
        CreateLabel(new Vector3(3f, -1.1f, 0), "t=1", 22, new Color(0.5f, 0.5f, 0.5f));

        lerpLabel = CreateLabel(new Vector3(0, -2.3f, 0), "", 34, Color.white);
        remapLabel = CreateLabel(new Vector3(0, -2.9f, 0), "", 30, new Color(0.5f, 0.8f, 1f));
    }

    public override void UpdateVisualization()
    {
        if (MathLineRenderer.Instance == null) return;
        var mr = MathLineRenderer.Instance;

        float t = Mathf.InverseLerp(-3f, 3f, handleT.LocalPosition.x);

        // 球沿 A→B 移動
        lerpBall.transform.localPosition = Vector3.Lerp(pointA, pointB, t);

        // 繪製 A→B 連線
        mr.DrawLine(transform.TransformPoint(pointA), transform.TransformPoint(pointB),
                    new Color(0.5f, 0.5f, 0.5f), 0.01f);

        // 繪製當前位置標記
        mr.DrawLine(transform.TransformPoint(lerpBall.transform.localPosition + Vector3.down * 0.3f),
                    transform.TransformPoint(lerpBall.transform.localPosition + Vector3.up * 0.3f),
                    new Color(1f, 0.9f, 0.3f, 0.5f), 0.025f);

        // 滑桿背景
        mr.DrawLine(transform.TransformPoint(new Vector3(-3, -1.5f, 0)),
                    transform.TransformPoint(new Vector3(3, -1.5f, 0)),
                    new Color(0.3f, 0.3f, 0.3f), 0.008f);

        // Remap 計算：t [0,1] → [-100, 100]
        float remapped = Mathf.Lerp(-100f, 100f, t);

        lerpLabel.text = $"t = {t:F3}    lerp 位置 = ({lerpBall.transform.localPosition.x:F2}, {lerpBall.transform.localPosition.y:F2})";
        remapLabel.text = $"Remap [0,1]→[-100,100] = {remapped:F1}";
    }

    public override bool CheckChallengeComplete()
    {
        float t = Mathf.InverseLerp(-3f, 3f, handleT.LocalPosition.x);
        return Mathf.Abs(t - 0.75f) < 0.02f;
    }

    protected override void OnChallengeCompleted()
    {
        base.OnChallengeCompleted();
        ChallengeSystem.Instance?.MarkCompleted(exhibitName);
    }
}
