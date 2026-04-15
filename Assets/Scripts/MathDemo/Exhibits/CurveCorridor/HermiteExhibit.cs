using UnityEngine;

// ============================================================
// HermiteExhibit.cs — Hermite 多項式曲線
// 拖端點 + 切線方向
// ============================================================

public class HermiteExhibit : ExhibitBase
{
    private DragHandle handleP0, handleP1, handleT0, handleT1;
    private TextMesh formulaLabel, tLabel;

    public override void BuildExhibit()
    {
        exhibitName = "Hermite 多項式";
        description = "Hermite 曲線用兩個端點 + 兩條切線定義：\n\nH(t) = h00·P0 + h10·T0 + h01·P1 + h11·T1\n\n🎮 遊戲應用：\n• Unity AnimationCurve 的關鍵幀插值方式\n• 角色移動路徑：指定起終點+方向\n• 地形道路：控制入口/出口角度\n• 比 Bezier 更直觀（直接控制切線方向）\n\n拖曳端點(藍/紅)和切線(淺色)";
        formula = "H(t) = (2t³-3t²+1)P₀ + (t³-2t²+t)T₀ + (-2t³+3t²)P₁ + (t³-t²)T₁";
        challengeDescription = "做出 S 型曲線（切線反向）";

        handleP0 = CreateDragHandle(new Vector3(-2f, 0, 0), new Color(0.3f, 0.7f, 1f));
        handleP1 = CreateDragHandle(new Vector3(2f, 0, 0), new Color(1f, 0.4f, 0.3f));
        handleT0 = CreateDragHandle(new Vector3(-1f, 1.5f, 0), new Color(0.5f, 0.8f, 1f), 0.1f);
        handleT1 = CreateDragHandle(new Vector3(1f, -1.5f, 0), new Color(1f, 0.7f, 0.6f), 0.1f);

        formulaLabel = CreateLabel(new Vector3(0, -2f, 0), "", 24, new Color(0.7f, 0.7f, 0.8f));
        tLabel = CreateLabel(new Vector3(0, -2.6f, 0), "", 22, new Color(0.6f, 0.6f, 0.7f));
    }

    public override void UpdateVisualization()
    {
        var mr = MathLineRenderer.Instance;
        if (mr == null) return;

        Vector3 p0 = handleP0.LocalPosition;
        Vector3 p1 = handleP1.LocalPosition;
        Vector3 t0 = (handleT0.LocalPosition - p0) * 2f;
        Vector3 t1 = (handleT1.LocalPosition - p1) * 2f;

        // 畫切線指示
        mr.DrawLine(transform.TransformPoint(p0), transform.TransformPoint(handleT0.LocalPosition), new Color(0.5f, 0.8f, 1f, 0.5f), 0.006f);
        mr.DrawLine(transform.TransformPoint(p1), transform.TransformPoint(handleT1.LocalPosition), new Color(1f, 0.7f, 0.6f, 0.5f), 0.006f);

        // Hermite 曲線
        int steps = 40;
        for (int i = 0; i < steps; i++)
        {
            float ta = (float)i / steps;
            float tb = (float)(i + 1) / steps;
            Vector3 a = HermitePoint(p0, p1, t0, t1, ta);
            Vector3 b = HermitePoint(p0, p1, t0, t1, tb);
            float hue = ta;
            Color c = Color.HSVToRGB(hue * 0.3f + 0.5f, 0.8f, 1f);
            mr.DrawLine(transform.TransformPoint(a), transform.TransformPoint(b), c, 0.015f);
        }

        formulaLabel.text = $"P0={Fmt(p0)} T0={Fmt(t0)} P1={Fmt(p1)} T1={Fmt(t1)}";
    }

    private Vector3 HermitePoint(Vector3 p0, Vector3 p1, Vector3 t0, Vector3 t1, float t)
    {
        float t2 = t * t, t3 = t2 * t;
        float h00 = 2 * t3 - 3 * t2 + 1;
        float h10 = t3 - 2 * t2 + t;
        float h01 = -2 * t3 + 3 * t2;
        float h11 = t3 - t2;
        return h00 * p0 + h10 * t0 + h01 * p1 + h11 * t1;
    }

    private string Fmt(Vector3 v) => $"({v.x:F1},{v.y:F1})";

    public override bool CheckChallengeComplete()
    {
        // S 型 = 切線方向大致相反
        Vector3 t0dir = handleT0.LocalPosition - handleP0.LocalPosition;
        Vector3 t1dir = handleT1.LocalPosition - handleP1.LocalPosition;
        return Vector3.Dot(t0dir.normalized, t1dir.normalized) < -0.3f;
    }
}
