using UnityEngine;

// ============================================================
// DistanceExhibit.cs — 距離計算
// 歐幾里得 / 曼哈頓 / 切比雪夫 三種距離對比
// ============================================================

public class DistanceExhibit : ExhibitBase
{
    private DragHandle handleA, handleB;
    private TextMesh euclidLabel, manhattanLabel, chebyshevLabel, titleLabel;
    private MathLineRenderer mr;

    public override void BuildExhibit()
    {
        exhibitName = "距離計算 Distance";
        description = "三種常用距離量度：\n\n• 歐幾里得：sqrt(dx²+dy²) — 直線距離\n• 曼哈頓：|dx|+|dy| — 格子街道距離\n• 切比雪夫：max(|dx|,|dy|) — 國王移動距離\n\n🎮 遊戲應用：\n• 仇恨範圍：distance < aggroRange\n• A* 啟發式：曼哈頓=格子地圖、歐幾里得=自由移動\n• 技能範圍判定：圓形/菱形/方形\n\n拖曳兩點觀察差異";
        formula = "d_E = √(Δx²+Δy²)\nd_M = |Δx|+|Δy|\nd_C = max(|Δx|,|Δy|)";
        challengeDescription = "讓三種距離值彼此不同";

        handleA = CreateDragHandle(new Vector3(-1.5f, 0, 0), new Color(0.3f, 0.8f, 1f));
        handleB = CreateDragHandle(new Vector3(1.5f, 1f, 0), new Color(1f, 0.5f, 0.3f));

        titleLabel = CreateLabel(new Vector3(0, 2.5f, 0), "距離對比", 36, Color.white);
        euclidLabel = CreateLabel(new Vector3(0, -1.5f, 0), "", 30, new Color(0.3f, 0.9f, 1f));
        manhattanLabel = CreateLabel(new Vector3(0, -2.1f, 0), "", 30, new Color(1f, 0.8f, 0.3f));
        chebyshevLabel = CreateLabel(new Vector3(0, -2.7f, 0), "", 30, new Color(0.5f, 1f, 0.5f));
    }

    public override void UpdateVisualization()
    {
        mr = MathLineRenderer.Instance;
        if (mr == null) return;

        Vector3 a = transform.TransformPoint(handleA.LocalPosition);
        Vector3 b = transform.TransformPoint(handleB.LocalPosition);
        Vector3 la = handleA.LocalPosition;
        Vector3 lb = handleB.LocalPosition;

        float dx = Mathf.Abs(lb.x - la.x);
        float dy = Mathf.Abs(lb.y - la.y);

        float eucDist = Vector3.Distance(la, lb);
        float manDist = dx + dy;
        float cheDist = Mathf.Max(dx, dy);

        // 直線
        mr.DrawLine(a, b, new Color(0.3f, 0.9f, 1f), 0.015f);

        // 曼哈頓路徑
        Vector3 corner = transform.TransformPoint(new Vector3(lb.x, la.y, la.z));
        mr.DrawLine(a, corner, new Color(1f, 0.8f, 0.3f), 0.01f);
        mr.DrawLine(corner, b, new Color(1f, 0.8f, 0.3f), 0.01f);

        euclidLabel.text = $"歐幾里得: {eucDist:F2}";
        manhattanLabel.text = $"曼哈頓: {manDist:F2}";
        chebyshevLabel.text = $"切比雪夫: {cheDist:F2}";
    }

    public override bool CheckChallengeComplete()
    {
        Vector3 la = handleA.LocalPosition, lb = handleB.LocalPosition;
        float dx = Mathf.Abs(lb.x - la.x), dy = Mathf.Abs(lb.y - la.y);
        float e = Mathf.Sqrt(dx * dx + dy * dy), m = dx + dy, c = Mathf.Max(dx, dy);
        return Mathf.Abs(e - m) > 0.3f && Mathf.Abs(m - c) > 0.3f && Mathf.Abs(e - c) > 0.3f;
    }
}
