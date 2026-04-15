using UnityEngine;

// ============================================================
// DotProductExhibit.cs — 點積互動展品
// 拖曳兩向量端點，即時顯示 a·b、cosθ、角度值
// 挑戰：讓兩向量垂直（點積 = 0）
// ============================================================

public class DotProductExhibit : ExhibitBase
{
    private DragHandle handleA;
    private DragHandle handleB;
    private TextMesh dotLabel;
    private TextMesh angleLabel;

    public override void BuildExhibit()
    {
        exhibitName = "點積 Dot Product";
        description = "a·b = |a||b|cosθ\n\n點積表示兩向量的相似程度：\n• 同向 → 正值\n• 垂直 → 零\n• 反向 → 負值\n\n🎮 遊戲應用：\n• FPS 敵人偵測：dot > 0 = 在前方\n• Lambert 漫反射光照：亮度 = max(0, N·L)\n• 自動瞥準：找 dot 最大的目標\n\n拖曳紅藍球體改變向量方向";
        formula = "a · b = ax*bx + ay*by";
        challengeDescription = "讓兩向量互相垂直（點積 ≈ 0）";

        // 原點標記
        CreateStaticPrimitive(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.12f, Color.white);

        // 拖曳端點
        handleA = CreateDragHandle(new Vector3(2f, 1f, 0), new Color(1f, 0.3f, 0.3f), 0.18f);
        handleB = CreateDragHandle(new Vector3(1f, 2f, 0), new Color(0.3f, 0.5f, 1f), 0.18f);

        // 數值顯示
        dotLabel = CreateLabel(new Vector3(0, -1.8f, 0), "", 40, Color.white);
        angleLabel = CreateLabel(new Vector3(0, -2.3f, 0), "", 35, new Color(0.7f, 0.8f, 1f));

        // 座標軸標籤
        CreateLabel(new Vector3(3.5f, -0.2f, 0), "X", 30, new Color(0.5f, 0.5f, 0.5f));
        CreateLabel(new Vector3(-0.3f, 3.5f, 0), "Y", 30, new Color(0.5f, 0.5f, 0.5f));
    }

    public override void UpdateVisualization()
    {
        if (MathLineRenderer.Instance == null) return;

        var mr = MathLineRenderer.Instance;
        Vector3 origin = transform.position;
        Vector3 a = handleA.LocalPosition;
        Vector3 b = handleB.LocalPosition;
        Vector3 worldA = transform.TransformPoint(a);
        Vector3 worldB = transform.TransformPoint(b);

        // 向量箭頭
        mr.DrawArrow(origin, worldA, new Color(1f, 0.3f, 0.3f), 0.035f);
        mr.DrawArrow(origin, worldB, new Color(0.3f, 0.5f, 1f), 0.035f);

        // 投影虛線
        float dot = Vector3.Dot(a, b);
        float bSqr = Vector3.Dot(b, b);
        if (bSqr > 0.001f)
        {
            Vector3 proj = (dot / bSqr) * b;
            Vector3 worldProj = transform.TransformPoint(proj);
            mr.DrawDashedLine(worldA, worldProj, new Color(1f, 1f, 0.3f, 0.5f));
            mr.DrawDashedLine(origin, worldProj, new Color(1f, 1f, 0.3f, 0.3f));
        }

        // 角度弧
        float magA = a.magnitude;
        float magB = b.magnitude;
        if (magA > 0.01f && magB > 0.01f)
        {
            float cosAngle = Mathf.Clamp(dot / (magA * magB), -1f, 1f);
            float angle = Mathf.Acos(cosAngle) * Mathf.Rad2Deg;

            Vector3 fromDir = a.normalized;
            mr.DrawArc(origin, fromDir, angle, 0.6f, new Color(1f, 0.85f, 0.3f), 0.02f);

            // 更新標籤
            dotLabel.text = $"a · b = {dot:F2}";
            angleLabel.text = $"θ = {angle:F1}°    cos θ = {cosAngle:F3}";

            // 垂直時高亮
            if (Mathf.Abs(dot) < 0.15f)
                dotLabel.color = new Color(0.3f, 1f, 0.3f);
            else
                dotLabel.color = Color.white;
        }

        // 座標軸
        mr.DrawLine(origin + Vector3.left * 3.5f, origin + Vector3.right * 3.5f, new Color(0.3f, 0.3f, 0.3f), 0.01f);
        mr.DrawLine(origin + Vector3.down * 3.5f, origin + Vector3.up * 3.5f, new Color(0.3f, 0.3f, 0.3f), 0.01f);
    }

    public override bool CheckChallengeComplete()
    {
        Vector3 a = handleA.LocalPosition;
        Vector3 b = handleB.LocalPosition;
        float dot = Vector3.Dot(a, b);
        return Mathf.Abs(dot) < 0.1f && a.magnitude > 0.5f && b.magnitude > 0.5f;
    }

    protected override void OnChallengeCompleted()
    {
        base.OnChallengeCompleted();
        ChallengeSystem.Instance?.MarkCompleted(exhibitName);
    }
}
