using UnityEngine;

// ============================================================
// CubicSplineExhibit.cs — 三次貝茲/樣條曲線
// 拖動 4 個控制點，即時顯示曲線
// ============================================================

public class CubicSplineExhibit : ExhibitBase
{
    private DragHandle[] handles = new DragHandle[4];
    private TextMesh formulaLabel;
    private GameObject movingBall;
    private float animT = 0;

    public override void BuildExhibit()
    {
        exhibitName = "三次樣條線 Cubic Spline";
        description = "P(t) = (1-t)³P₀ + 3(1-t)²tP₁ + 3(1-t)t²P₂ + t³P₃\n\n三次貝茲曲線用 4 個控制點定義\n\n🎮 遊戲應用：\n• 過場動畫的鏡頭平滑路徑\n• 賽車遊戲的賽道生成\n• Unity AnimationCurve 底層原理\n• UI 動畫的加速/減速曲線\n\n拖曳 4 個控制點觀察曲線變化\n黃球沿曲線移動";
        formula = "B(t) = Σ C(3,i) × (1-t)^(3-i) × t^i × Pᵢ";
        challengeDescription = "讓曲線形成 S 形（P1 在上、P2 在下）";

        handles[0] = CreateDragHandle(new Vector3(-3, -1, 0), new Color(1f, 0.3f, 0.3f), 0.17f);
        handles[1] = CreateDragHandle(new Vector3(-1, 2, 0), new Color(0.4f, 0.9f, 0.4f), 0.17f);
        handles[2] = CreateDragHandle(new Vector3(1, -2, 0), new Color(0.4f, 0.4f, 0.9f), 0.17f);
        handles[3] = CreateDragHandle(new Vector3(3, 1, 0), new Color(1f, 0.7f, 0.3f), 0.17f);

        CreateLabel(new Vector3(-3, -1.6f, 0), "P₀", 25, new Color(1f, 0.4f, 0.4f));
        CreateLabel(new Vector3(-1, 2.5f, 0), "P₁", 25, new Color(0.4f, 0.9f, 0.4f));
        CreateLabel(new Vector3(1, -2.7f, 0), "P₂", 25, new Color(0.4f, 0.4f, 0.9f));
        CreateLabel(new Vector3(3, 1.5f, 0), "P₃", 25, new Color(1f, 0.7f, 0.3f));

        movingBall = CreateStaticPrimitive(PrimitiveType.Sphere,
            Vector3.zero, Vector3.one * 0.25f, new Color(1f, 0.95f, 0.3f));

        formulaLabel = CreateLabel(new Vector3(0, -3.5f, 0), "", 28, Color.white);
    }

    public override void UpdateVisualization()
    {
        if (MathLineRenderer.Instance == null) return;
        var mr = MathLineRenderer.Instance;

        Vector3 p0 = handles[0].LocalPosition;
        Vector3 p1 = handles[1].LocalPosition;
        Vector3 p2 = handles[2].LocalPosition;
        Vector3 p3 = handles[3].LocalPosition;

        // 控制線（虛線）
        mr.DrawDashedLine(transform.TransformPoint(p0), transform.TransformPoint(p1), new Color(0.5f, 0.5f, 0.5f, 0.4f));
        mr.DrawDashedLine(transform.TransformPoint(p2), transform.TransformPoint(p3), new Color(0.5f, 0.5f, 0.5f, 0.4f));

        // 曲線
        int segments = 40;
        for (int i = 0; i < segments; i++)
        {
            float t0 = (float)i / segments;
            float t1 = (float)(i + 1) / segments;
            Vector3 a = CubicBezier(p0, p1, p2, p3, t0);
            Vector3 b = CubicBezier(p0, p1, p2, p3, t1);

            Color lineColor = Color.Lerp(new Color(1f, 0.3f, 0.3f), new Color(0.3f, 0.5f, 1f), t0);
            mr.DrawLine(transform.TransformPoint(a), transform.TransformPoint(b), lineColor, 0.025f);
        }

        // 動畫球
        animT += Time.deltaTime * 0.3f;
        if (animT > 1f) animT -= 1f;
        movingBall.transform.localPosition = CubicBezier(p0, p1, p2, p3, animT);

        formulaLabel.text = $"t = {animT:F2}  位置 = ({movingBall.transform.localPosition.x:F1}, {movingBall.transform.localPosition.y:F1})";
    }

    private Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1f - t;
        return u * u * u * p0 + 3f * u * u * t * p1 + 3f * u * t * t * p2 + t * t * t * p3;
    }

    public override bool CheckChallengeComplete()
    {
        // S 形：P1 在上半部、P2 在下半部
        return handles[1].LocalPosition.y > 1f && handles[2].LocalPosition.y < -1f;
    }

    protected override void OnChallengeCompleted()
    {
        base.OnChallengeCompleted();
        ChallengeSystem.Instance?.MarkCompleted(exhibitName);
    }
}
