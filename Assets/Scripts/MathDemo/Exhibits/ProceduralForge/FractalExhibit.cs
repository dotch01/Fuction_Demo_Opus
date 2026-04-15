using UnityEngine;

// ============================================================
// FractalExhibit.cs — 碎形
// Koch 雪花逐級細分
// ============================================================

public class FractalExhibit : ExhibitBase
{
    private DragHandle handleLevel;
    private TextMesh levelLabel, statsLabel;

    public override void BuildExhibit()
    {
        exhibitName = "碎形 Fractal";
        description = "Koch 雪花 — 自相似碎形：\n\n每條線段 → 分成三等份\n中間替換為等邊三角形突起\n\n每次迭代：邊數 ×4、周長 ×4/3\n面積趨近有限值 → 無限細節！\n\n🎮 遊戲應用：\n• 程序化海岸線/山脈輪廓生成\n• 分形火焰/雷電效果\n• 無限放大的程序化紋理\n• 地形粗糙度模擬\n\n拖曳控制迭代層級";
        formula = "邊數 = 3·4ⁿ    周長 = 3·(4/3)ⁿ → ∞";
        challengeDescription = "讓迭代到 Level 4";

        handleLevel = CreateDragHandle(new Vector3(0, -2.2f, 0), new Color(0.3f, 0.8f, 1f), 0.12f);
        handleLevel.minBounds = new Vector3(-2f, -2.2f, 0);
        handleLevel.maxBounds = new Vector3(2f, -2.2f, 0);

        CreateLabel(new Vector3(-2.3f, -2.2f, 0), "Lv0", 20, new Color(0.5f, 0.5f, 0.6f));
        CreateLabel(new Vector3(2.3f, -2.2f, 0), "Lv5", 20, new Color(0.5f, 0.5f, 0.6f));

        levelLabel = CreateLabel(new Vector3(0, -2.9f, 0), "", 30, Color.white);
        statsLabel = CreateLabel(new Vector3(0, -3.5f, 0), "", 24, new Color(0.7f, 0.7f, 0.8f));
    }

    public override void UpdateVisualization()
    {
        var mr = MathLineRenderer.Instance;
        if (mr == null) return;

        int level = Mathf.RoundToInt(Mathf.Lerp(0, 5, Mathf.InverseLerp(-2f, 2f, handleLevel.LocalPosition.x)));
        level = Mathf.Clamp(level, 0, 5);

        // 初始等邊三角形
        float size = 2.5f;
        float h = size * Mathf.Sqrt(3f) / 2f;
        Vector3 p0 = new Vector3(-size / 2f, -h / 3f, 0);
        Vector3 p1 = new Vector3(size / 2f, -h / 3f, 0);
        Vector3 p2 = new Vector3(0, 2f * h / 3f, 0);

        DrawKoch(mr, p0, p1, level);
        DrawKoch(mr, p1, p2, level);
        DrawKoch(mr, p2, p0, level);

        int edges = 3 * (int)Mathf.Pow(4, level);
        float perimeter = 3f * Mathf.Pow(4f / 3f, level);
        levelLabel.text = $"Level {level}";
        statsLabel.text = $"邊數 = {edges}    周長 ∝ {perimeter:F2}";
    }

    private void DrawKoch(MathLineRenderer mr, Vector3 a, Vector3 b, int depth)
    {
        if (depth == 0)
        {
            mr.DrawLine(transform.TransformPoint(a), transform.TransformPoint(b), new Color(0.3f, 0.8f, 1f), 0.008f);
            return;
        }

        Vector3 d = b - a;
        Vector3 p1 = a + d / 3f;
        Vector3 p2 = a + d * 2f / 3f;

        // 突起頂點（向外）
        Vector3 mid = (p1 + p2) / 2f;
        Vector3 perp = new Vector3(-d.y, d.x, 0).normalized;
        Vector3 peak = mid + perp * d.magnitude / (3f * Mathf.Sqrt(3f)) * (-1);

        // 反轉為向外
        peak = mid + perp * d.magnitude * Mathf.Sqrt(3f) / 6f;

        DrawKoch(mr, a, p1, depth - 1);
        DrawKoch(mr, p1, peak, depth - 1);
        DrawKoch(mr, peak, p2, depth - 1);
        DrawKoch(mr, p2, b, depth - 1);
    }

    public override bool CheckChallengeComplete()
    {
        int level = Mathf.RoundToInt(Mathf.Lerp(0, 5, Mathf.InverseLerp(-2f, 2f, handleLevel.LocalPosition.x)));
        return level >= 4;
    }
}
