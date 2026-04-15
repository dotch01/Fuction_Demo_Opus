using UnityEngine;

// ============================================================
// ClosestOnCircleExhibit.cs — 圓上最近點
// ============================================================

public class ClosestOnCircleExhibit : ExhibitBase
{
    private DragHandle handlePoint;
    private TextMesh resultLabel, formulaLabel;
    private float radius = 1.5f;
    private Vector3 circleCenter = new Vector3(0, 0.5f, 0);

    public override void BuildExhibit()
    {
        exhibitName = "圓上最近點 Closest on Circle";
        description = "給一個外部點 P 和圓心 C、半徑 r\n\n最近點 = C + normalize(P - C) × r\n\n🎮 遊戲應用：\n• 圓形攻擊範圍的邊界指示器\n• 砲塔自動瞥準最近邊界點\n• 圓形碰撞修正：推出重疊\n• MOBA 技能範圍預覽邊緣\n\n拖曳點觀察最近點變化";
        formula = "Q = C + normalize(P - C) · r";
        challengeDescription = "把外部點拖到距圓心超過 3";

        handlePoint = CreateDragHandle(new Vector3(2f, 1.5f, 0), new Color(1f, 0.5f, 0.3f));

        resultLabel = CreateLabel(new Vector3(0, -2f, 0), "", 28, Color.white);
        formulaLabel = CreateLabel(new Vector3(0, -2.6f, 0), "", 24, new Color(0.7f, 0.7f, 0.8f));
    }

    public override void UpdateVisualization()
    {
        var mr = MathLineRenderer.Instance;
        if (mr == null) return;

        Vector3 cw = transform.TransformPoint(circleCenter);
        mr.DrawCircle(cw, radius, transform.forward, Color.cyan, 0.01f, 48);

        Vector3 p = transform.TransformPoint(handlePoint.LocalPosition);
        Vector3 dir = (p - cw);
        float dist = dir.magnitude;
        Vector3 closest = cw + (dist > 0.001f ? dir / dist : Vector3.right) * radius;

        // 連線
        mr.DrawLine(cw, p, new Color(0.4f, 0.4f, 0.5f), 0.008f);
        mr.DrawLine(p, closest, new Color(1f, 0.85f, 0.2f), 0.012f);

        // 最近點球
        mr.DrawLine(closest - Vector3.right * 0.04f, closest + Vector3.right * 0.04f, new Color(0.3f, 1f, 0.5f), 0.04f);

        float distToCircle = Mathf.Abs(dist - radius);
        resultLabel.text = $"最近點距離: {distToCircle:F2}    P 在圓{(dist > radius ? "外" : dist < radius ? "內" : "上")}";
        formulaLabel.text = $"Q = C + d̂ · {radius:F1}";
    }

    public override bool CheckChallengeComplete()
    {
        return Vector3.Distance(handlePoint.LocalPosition, circleCenter) > 3f;
    }
}
