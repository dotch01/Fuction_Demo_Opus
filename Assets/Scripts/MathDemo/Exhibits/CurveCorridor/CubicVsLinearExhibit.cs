using UnityEngine;

// ============================================================
// CubicVsLinearExhibit.cs — 三次插值 vs 線性
// 四個資料點，比較兩種插值的平滑度
// ============================================================

public class CubicVsLinearExhibit : ExhibitBase
{
    private DragHandle[] handles = new DragHandle[4];
    private TextMesh infoLabel;

    public override void BuildExhibit()
    {
        exhibitName = "三次 vs 線性插值";
        description = "同樣 4 個控制點：\n\n• 線性插值（白色）：直連折線\n• 三次插值（彩色）：Catmull-Rom 平滑曲線\n\n🎮 遊戲應用：\n• 過場鏡頭路徑：折線太生硬，曲線才自然\n• NPC 巡邏路線：經過 waypoints 平滑轉彎\n• 地形道路生成：用控制點自動產生平滑路\n\n拖曳 4 個點觀察差異";
        formula = "Catmull-Rom: q(t) 通過每個控制點，一階連續";
        challengeDescription = "讓 4 點形成明顯 S 曲線";

        float[] xs = { -2.5f, -0.8f, 0.8f, 2.5f };
        Color[] colors = {
            new Color(0.3f, 0.8f, 1f), new Color(0.5f, 1f, 0.5f),
            new Color(1f, 0.8f, 0.3f), new Color(1f, 0.4f, 0.4f)
        };

        for (int i = 0; i < 4; i++)
        {
            handles[i] = CreateDragHandle(new Vector3(xs[i], (i % 2 == 0 ? 0.5f : -0.5f), 0), colors[i]);
        }

        infoLabel = CreateLabel(new Vector3(0, -2f, 0), "白 = 線性　彩色 = Catmull-Rom 三次", 26, new Color(0.7f, 0.7f, 0.8f));
    }

    public override void UpdateVisualization()
    {
        var mr = MathLineRenderer.Instance;
        if (mr == null) return;

        Vector3[] pts = new Vector3[4];
        for (int i = 0; i < 4; i++) pts[i] = handles[i].LocalPosition;

        // 線性
        for (int i = 0; i < 3; i++)
        {
            mr.DrawLine(transform.TransformPoint(pts[i]), transform.TransformPoint(pts[i + 1]),
                new Color(0.7f, 0.7f, 0.7f), 0.008f);
        }

        // Catmull-Rom
        for (int seg = 0; seg < 3; seg++)
        {
            Vector3 p0 = pts[Mathf.Max(seg - 1, 0)];
            Vector3 p1 = pts[seg];
            Vector3 p2 = pts[Mathf.Min(seg + 1, 3)];
            Vector3 p3 = pts[Mathf.Min(seg + 2, 3)];

            int steps = 20;
            for (int i = 0; i < steps; i++)
            {
                float ta = (float)i / steps;
                float tb = (float)(i + 1) / steps;
                Vector3 a = CatmullRom(p0, p1, p2, p3, ta);
                Vector3 b = CatmullRom(p0, p1, p2, p3, tb);
                float hue = (seg + ta) / 3f;
                mr.DrawLine(transform.TransformPoint(a), transform.TransformPoint(b),
                    Color.HSVToRGB(hue * 0.5f, 0.9f, 1f), 0.015f);
            }
        }
    }

    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t, t3 = t2 * t;
        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }

    public override bool CheckChallengeComplete()
    {
        // S 曲線 = 交替高低
        float y0 = handles[0].LocalPosition.y, y1 = handles[1].LocalPosition.y;
        float y2 = handles[2].LocalPosition.y, y3 = handles[3].LocalPosition.y;
        return (y0 - y1) * (y2 - y1) < 0 && (y1 - y2) * (y3 - y2) < 0;
    }
}
