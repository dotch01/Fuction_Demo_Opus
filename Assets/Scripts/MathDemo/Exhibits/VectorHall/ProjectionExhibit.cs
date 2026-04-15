using UnityEngine;

// ============================================================
// ProjectionExhibit.cs — 向量投影
// 拖曳 A 和 B，顯示 A 投影到 B 上的分量和垂直分量
// 挑戰：讓投影長度 > 2
// ============================================================

public class ProjectionExhibit : ExhibitBase
{
    private DragHandle handleA;
    private DragHandle handleB;
    private TextMesh projLabel;
    private TextMesh perpLabel;

    public override void BuildExhibit()
    {
        exhibitName = "向量投影 Projection";
        description = "proj_b(a) = (a·b / b·b) × b\n\n將 A 向量分解為：\n• 平行於 B 的投影分量（黃色）\n• 垂直於 B 的分量（綠色虛線）\n\n🎮 遊戲應用：\n• 斜面上的滑動力分解\n• 角色沿牆壁滑動（壁跑）\n• 速度分解：水平/垂直分量\n\n拖曳觀察投影如何變化";
        formula = "proj = (a·b / |b|²) × b";
        challengeDescription = "讓投影長度 > 2";

        CreateStaticPrimitive(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.12f, Color.white);

        handleA = CreateDragHandle(new Vector3(1.5f, 2.5f, 0), new Color(1f, 0.3f, 0.3f), 0.18f);
        handleB = CreateDragHandle(new Vector3(3f, 0.5f, 0), new Color(0.3f, 0.5f, 1f), 0.18f);

        projLabel = CreateLabel(new Vector3(0, -2f, 0), "", 36, new Color(1f, 0.9f, 0.3f));
        perpLabel = CreateLabel(new Vector3(0, -2.5f, 0), "", 34, new Color(0.3f, 1f, 0.5f));
    }

    public override void UpdateVisualization()
    {
        if (MathLineRenderer.Instance == null) return;
        var mr = MathLineRenderer.Instance;

        Vector3 origin = transform.position;
        Vector3 a = handleA.LocalPosition;
        Vector3 b = handleB.LocalPosition;

        mr.DrawArrow(origin, transform.TransformPoint(a), new Color(1f, 0.3f, 0.3f), 0.035f);
        mr.DrawArrow(origin, transform.TransformPoint(b), new Color(0.3f, 0.5f, 1f), 0.035f);

        float bSqr = Vector3.Dot(b, b);
        if (bSqr > 0.001f)
        {
            float dot = Vector3.Dot(a, b);
            Vector3 proj = (dot / bSqr) * b;
            Vector3 perp = a - proj;

            Vector3 worldProj = transform.TransformPoint(proj);

            // 投影向量（黃色）
            mr.DrawArrow(origin, worldProj, new Color(1f, 0.9f, 0.3f), 0.04f);

            // 垂直分量（綠色虛線）
            mr.DrawDashedLine(worldProj, transform.TransformPoint(a), new Color(0.3f, 1f, 0.5f));

            // 直角標記
            if (perp.magnitude > 0.1f && proj.magnitude > 0.1f)
            {
                Vector3 cornerDir1 = perp.normalized * 0.2f;
                Vector3 cornerDir2 = proj.normalized * 0.2f;
                Vector3 corner = proj;
                mr.DrawLine(
                    transform.TransformPoint(corner + cornerDir1),
                    transform.TransformPoint(corner + cornerDir1 + cornerDir2),
                    new Color(1f, 1f, 1f, 0.4f), 0.01f);
                mr.DrawLine(
                    transform.TransformPoint(corner + cornerDir2),
                    transform.TransformPoint(corner + cornerDir1 + cornerDir2),
                    new Color(1f, 1f, 1f, 0.4f), 0.01f);
            }

            projLabel.text = $"投影 |proj| = {proj.magnitude:F2}";
            perpLabel.text = $"垂直 |perp| = {perp.magnitude:F2}";
        }
    }

    public override bool CheckChallengeComplete()
    {
        Vector3 a = handleA.LocalPosition;
        Vector3 b = handleB.LocalPosition;
        float bSqr = Vector3.Dot(b, b);
        if (bSqr < 0.001f) return false;
        Vector3 proj = (Vector3.Dot(a, b) / bSqr) * b;
        return proj.magnitude > 2f;
    }

    protected override void OnChallengeCompleted()
    {
        base.OnChallengeCompleted();
        ChallengeSystem.Instance?.MarkCompleted(exhibitName);
    }
}
