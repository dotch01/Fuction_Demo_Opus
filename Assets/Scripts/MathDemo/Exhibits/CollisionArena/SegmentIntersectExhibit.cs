using UnityEngine;

// ============================================================
// SegmentIntersectExhibit.cs — 線段相交
// 拖四端點，即時顯示交點
// ============================================================

public class SegmentIntersectExhibit : ExhibitBase
{
    private DragHandle handleA1, handleA2, handleB1, handleB2;
    private TextMesh resultLabel, paramLabel;

    public override void BuildExhibit()
    {
        exhibitName = "線段相交 Segment Intersect";
        description = "兩條線段 A₁A₂ 和 B₁B₂ 是否相交？\n\n用參數式：P = A₁ + t(A₂-A₁)\n聯立求解 t 和 u\n0 ≤ t ≤ 1 且 0 ≤ u ≤ 1 → 相交！\n\n🎮 遊戲應用：\n• 2D 物理碰撞偵測（線段 vs 線段）\n• AI 視線檢查：到玩家的線段是否穿牆\n• 雷射光束碰到障礙物\n• 觸發線/絆線機制\n\n拖曳四個端點";
        formula = "t = (B₁-A₁)×d_B / (d_A×d_B)\nu = (B₁-A₁)×d_A / (d_A×d_B)";
        challengeDescription = "讓兩線段交叉";

        handleA1 = CreateDragHandle(new Vector3(-2f, -1f, 0), new Color(0.3f, 0.8f, 1f));
        handleA2 = CreateDragHandle(new Vector3(2f, 1f, 0), new Color(0.4f, 0.85f, 1f));
        handleB1 = CreateDragHandle(new Vector3(-1f, 1.5f, 0), new Color(1f, 0.5f, 0.3f));
        handleB2 = CreateDragHandle(new Vector3(1f, -1.5f, 0), new Color(1f, 0.6f, 0.4f));

        resultLabel = CreateLabel(new Vector3(0, -2.2f, 0), "", 30, Color.white);
        paramLabel = CreateLabel(new Vector3(0, -2.8f, 0), "", 24, new Color(0.7f, 0.7f, 0.8f));
    }

    public override void UpdateVisualization()
    {
        var mr = MathLineRenderer.Instance;
        if (mr == null) return;

        Vector3 a1 = handleA1.LocalPosition, a2 = handleA2.LocalPosition;
        Vector3 b1 = handleB1.LocalPosition, b2 = handleB2.LocalPosition;

        mr.DrawLine(transform.TransformPoint(a1), transform.TransformPoint(a2), new Color(0.3f, 0.8f, 1f), 0.012f);
        mr.DrawLine(transform.TransformPoint(b1), transform.TransformPoint(b2), new Color(1f, 0.5f, 0.3f), 0.012f);

        // 2D 交點計算
        Vector2 p = new Vector2(a1.x, a1.y), r = new Vector2(a2.x - a1.x, a2.y - a1.y);
        Vector2 q = new Vector2(b1.x, b1.y), s = new Vector2(b2.x - b1.x, b2.y - b1.y);

        float rxs = r.x * s.y - r.y * s.x;
        if (Mathf.Abs(rxs) < 0.0001f)
        {
            resultLabel.text = "平行（無交點）";
            resultLabel.color = new Color(0.6f, 0.6f, 0.7f);
            paramLabel.text = "";
            return;
        }

        Vector2 qp = q - p;
        float t = (qp.x * s.y - qp.y * s.x) / rxs;
        float u = (qp.x * r.y - qp.y * r.x) / rxs;

        bool intersects = t >= 0 && t <= 1 && u >= 0 && u <= 1;
        Vector2 hit = p + t * r;

        if (intersects)
        {
            Vector3 hitW = transform.TransformPoint(new Vector3(hit.x, hit.y, 0));
            mr.DrawLine(hitW - Vector3.right * 0.06f, hitW + Vector3.right * 0.06f, Color.yellow, 0.06f);
            resultLabel.text = $"✓ 交點 ({hit.x:F2}, {hit.y:F2})";
            resultLabel.color = new Color(0.3f, 1f, 0.5f);
        }
        else
        {
            resultLabel.text = "✗ 線段不相交";
            resultLabel.color = new Color(1f, 0.4f, 0.3f);
        }
        paramLabel.text = $"t = {t:F3}   u = {u:F3}   (需 0≤t,u≤1)";
    }

    public override bool CheckChallengeComplete()
    {
        Vector2 p = new Vector2(handleA1.LocalPosition.x, handleA1.LocalPosition.y);
        Vector2 r = new Vector2(handleA2.LocalPosition.x - p.x, handleA2.LocalPosition.y - p.y);
        Vector2 q = new Vector2(handleB1.LocalPosition.x, handleB1.LocalPosition.y);
        Vector2 s = new Vector2(handleB2.LocalPosition.x - q.x, handleB2.LocalPosition.y - q.y);
        float rxs = r.x * s.y - r.y * s.x;
        if (Mathf.Abs(rxs) < 0.0001f) return false;
        Vector2 qp = q - p;
        float t = (qp.x * s.y - qp.y * s.x) / rxs;
        float u = (qp.x * r.y - qp.y * r.x) / rxs;
        return t >= 0 && t <= 1 && u >= 0 && u <= 1;
    }
}
