using UnityEngine;

// ============================================================
// DirectionExhibit.cs — A→B 方向
// 方向向量 + 正規化 + 長度
// ============================================================

public class DirectionExhibit : ExhibitBase
{
    private DragHandle handleA, handleB;
    private TextMesh dirLabel, normLabel, lenLabel;

    public override void BuildExhibit()
    {
        exhibitName = "A→B 方向 Direction";
        description = "從 A 到 B 的方向向量：\n\ndirection = B - A\nnormalized = direction / |direction|\n\n正規化後長度 = 1\n\n🎮 遊戲應用：\n• 敵人 AI 追蹤玩家的移動方向\n• 子彈方向 = normalize(target - gun)\n• 相機 Follow：朝向目標的方向\n• NPC 面朝玩家說話\n\n拖曳 A、B 觀察方向變化";
        formula = "d̂ = (B-A) / ‖B-A‖";
        challengeDescription = "讓兩點距離超過 2";

        handleA = CreateDragHandle(new Vector3(-1f, 0, 0), new Color(0.3f, 0.8f, 1f));
        handleB = CreateDragHandle(new Vector3(1f, 0.5f, 0), new Color(1f, 0.5f, 0.3f));

        dirLabel = CreateLabel(new Vector3(0, -1.5f, 0), "", 28, Color.white);
        normLabel = CreateLabel(new Vector3(0, -2.1f, 0), "", 28, new Color(0.3f, 1f, 0.5f));
        lenLabel = CreateLabel(new Vector3(0, -2.7f, 0), "", 28, new Color(1f, 0.85f, 0.3f));
    }

    public override void UpdateVisualization()
    {
        var mr = MathLineRenderer.Instance;
        if (mr == null) return;

        Vector3 a = transform.TransformPoint(handleA.LocalPosition);
        Vector3 b = transform.TransformPoint(handleB.LocalPosition);
        Vector3 dir = b - a;
        float len = dir.magnitude;
        Vector3 norm = len > 0.001f ? dir / len : Vector3.zero;

        // 原始向量
        mr.DrawArrow(a, b, new Color(0.4f, 0.6f, 0.9f), 0.015f, 0.08f);

        // 正規化向量（從 A 出發 長度 1）
        if (len > 0.01f)
        {
            Vector3 normEnd = a + norm;
            mr.DrawArrow(a, normEnd, new Color(0.3f, 1f, 0.5f), 0.02f, 0.1f);
        }

        dirLabel.text = $"方向: ({dir.x:F2}, {dir.y:F2})";
        normLabel.text = $"正規化: ({norm.x:F2}, {norm.y:F2})  |d̂| = 1";
        lenLabel.text = $"距離: {len:F2}";
    }

    public override bool CheckChallengeComplete()
    {
        return Vector3.Distance(handleA.LocalPosition, handleB.LocalPosition) > 2f;
    }
}
