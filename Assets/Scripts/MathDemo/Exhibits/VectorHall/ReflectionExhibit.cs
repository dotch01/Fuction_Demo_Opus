using UnityEngine;

// ============================================================
// ReflectionExhibit.cs — 向量反射
// 拖曳入射向量，碰到法線平面後反射
// 挑戰：讓反射向量指向上方（y > 0.8）
// ============================================================

public class ReflectionExhibit : ExhibitBase
{
    private DragHandle handleIncident;
    private DragHandle handleNormal;
    private TextMesh resultLabel;

    public override void BuildExhibit()
    {
        exhibitName = "向量反射 Reflection";
        description = "r = d - 2(d·n)n\n\n入射向量碰到表面後反射\n• 紅色 = 入射方向 d\n• 綠色 = 法線 n（碰撞面的垂直方向）\n• 黃色 = 反射方向 r\n\n🎮 遊戲應用：\n• 撞球/彈珠台的球反彈物理\n• Ray Tracing 光線追蹤反射\n• 雷射謎題遊戲的光線路徑\n• 粒子碰牆後的反方向\n\n拖曳改變入射方向觀察反射";
        formula = "r = d - 2(d · n̂) × n̂";
        challengeDescription = "調整讓反射向量朝正上方";

        // 碰撞面（水平線）
        CreateStaticPrimitive(PrimitiveType.Cube,
            new Vector3(0, 0, 0), new Vector3(5f, 0.05f, 0.5f), new Color(0.4f, 0.4f, 0.5f));

        handleIncident = CreateDragHandle(new Vector3(-1.5f, 2.5f, 0), new Color(1f, 0.3f, 0.3f), 0.18f);
        handleNormal = CreateDragHandle(new Vector3(0f, 1.8f, 0), new Color(0.3f, 0.9f, 0.4f), 0.15f);

        resultLabel = CreateLabel(new Vector3(0, -2f, 0), "", 36, new Color(1f, 0.9f, 0.3f));
    }

    public override void UpdateVisualization()
    {
        if (MathLineRenderer.Instance == null) return;
        var mr = MathLineRenderer.Instance;

        Vector3 surfacePoint = transform.position; // 碰撞點在原點
        Vector3 incident = handleIncident.LocalPosition;
        Vector3 normal = handleNormal.LocalPosition.normalized;

        // 入射向量（從端點射向碰撞面）
        Vector3 incidentDir = -incident.normalized;
        Vector3 worldStart = transform.TransformPoint(incident);

        mr.DrawArrow(worldStart, surfacePoint, new Color(1f, 0.3f, 0.3f), 0.035f);

        // 法線
        mr.DrawArrow(surfacePoint, transform.TransformPoint(normal * 2f), new Color(0.3f, 0.9f, 0.4f), 0.03f);

        // 反射
        Vector3 reflection = incidentDir - 2f * Vector3.Dot(incidentDir, normal) * normal;
        Vector3 reflEnd = transform.TransformPoint(-reflection * incident.magnitude);

        mr.DrawArrow(surfacePoint, reflEnd, new Color(1f, 0.9f, 0.3f), 0.035f);

        // 入射角 = 反射角 標示
        float incAngle = Vector3.Angle(-incidentDir, normal);
        float refAngle = Vector3.Angle(reflection, normal);

        resultLabel.text = $"入射角 = {incAngle:F1}°    反射角 = {refAngle:F1}°";

        // 法線虛線延長
        mr.DrawDashedLine(surfacePoint, transform.TransformPoint(-normal * 2f), new Color(0.3f, 0.9f, 0.4f, 0.3f));
    }

    public override bool CheckChallengeComplete()
    {
        Vector3 incident = handleIncident.LocalPosition;
        Vector3 normal = handleNormal.LocalPosition.normalized;
        Vector3 incidentDir = -incident.normalized;
        Vector3 reflection = incidentDir - 2f * Vector3.Dot(incidentDir, normal) * normal;
        return reflection.y > 0.8f;
    }

    protected override void OnChallengeCompleted()
    {
        base.OnChallengeCompleted();
        ChallengeSystem.Instance?.MarkCompleted(exhibitName);
    }
}
