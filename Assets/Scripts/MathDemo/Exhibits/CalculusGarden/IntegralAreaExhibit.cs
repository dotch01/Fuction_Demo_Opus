using UnityEngine;

// ============================================================
// IntegralAreaExhibit.cs — 積分面積
// 拖積分區間，曲線下方即時填充
// ============================================================

public class IntegralAreaExhibit : ExhibitBase
{
    private DragHandle handleA, handleB;
    private TextMesh areaLabel, boundsLabel;

    public override void BuildExhibit()
    {
        exhibitName = "積分面積 Integration";
        description = "定積分 = 曲線下的面積\n\n∫ₐᵇ f(x)dx = F(b) - F(a)\n\nf(x) = x² → F(x) = x³/3\n\n🎮 遊戲應用：\n• 速度積分 = 距離（物體走了多遠）\n• DPS 計算：傷害曲線下面積 = 總傷害\n• 經驗值累積成長曲線\n• 動量/衝量計算\n\n拖曳控制區間 [a, b]\n藍=正面積  紅=負面積";
        formula = "∫ₐᵇ x² dx = b³/3 - a³/3";
        challengeDescription = "讓積分值恰好超過 5";

        handleA = CreateDragHandle(new Vector3(-1.5f, -2f, 0), new Color(0.3f, 0.8f, 1f), 0.12f);
        handleA.minBounds = new Vector3(-2.5f, -2f, 0);
        handleA.maxBounds = new Vector3(0, -2f, 0);

        handleB = CreateDragHandle(new Vector3(1.5f, -2f, 0), new Color(1f, 0.5f, 0.3f), 0.12f);
        handleB.minBounds = new Vector3(0, -2f, 0);
        handleB.maxBounds = new Vector3(2.5f, -2f, 0);

        CreateLabel(new Vector3(-2.8f, -2f, 0), "a", 22, new Color(0.5f, 0.5f, 0.6f));
        CreateLabel(new Vector3(2.8f, -2f, 0), "b", 22, new Color(0.5f, 0.5f, 0.6f));

        areaLabel = CreateLabel(new Vector3(0, -2.8f, 0), "", 30, Color.white);
        boundsLabel = CreateLabel(new Vector3(0, -3.4f, 0), "", 24, new Color(0.7f, 0.7f, 0.8f));
    }

    private float F(float x) => 0.4f * x * x; // 顯示用
    private float FAnalytic(float x) => 0.4f * x * x * x / 3f; // 反導

    public override void UpdateVisualization()
    {
        var mr = MathLineRenderer.Instance;
        if (mr == null) return;

        float aVal = Mathf.Lerp(-3f, 0, Mathf.InverseLerp(-2.5f, 0, handleA.LocalPosition.x));
        float bVal = Mathf.Lerp(0, 3f, Mathf.InverseLerp(0, 2.5f, handleB.LocalPosition.x));

        // 畫曲線
        int steps = 60;
        for (int i = 0; i < steps; i++)
        {
            float x0 = Mathf.Lerp(-3.5f, 3.5f, (float)i / steps);
            float x1 = Mathf.Lerp(-3.5f, 3.5f, (float)(i + 1) / steps);
            mr.DrawLine(transform.TransformPoint(new Vector3(x0 * 0.6f, F(x0) * 0.5f, 0)),
                        transform.TransformPoint(new Vector3(x1 * 0.6f, F(x1) * 0.5f, 0)),
                        new Color(0.3f, 0.5f, 0.8f), 0.01f);
        }

        // 填充面積
        int fillSteps = 40;
        for (int i = 0; i < fillSteps; i++)
        {
            float x = Mathf.Lerp(aVal, bVal, (float)i / fillSteps);
            float y = F(x);
            float screenX = x * 0.6f;
            Color fill = y >= 0 ? new Color(0.3f, 0.6f, 1f, 0.4f) : new Color(1f, 0.3f, 0.3f, 0.4f);
            mr.DrawLine(transform.TransformPoint(new Vector3(screenX, 0, 0)),
                        transform.TransformPoint(new Vector3(screenX, y * 0.5f, 0)),
                        fill, 0.02f);
        }

        // X 軸
        mr.DrawLine(transform.TransformPoint(new Vector3(-2.5f, 0, 0)),
                    transform.TransformPoint(new Vector3(2.5f, 0, 0)),
                    new Color(0.4f, 0.4f, 0.5f), 0.005f);

        // 界線
        float sa = aVal * 0.6f, sb = bVal * 0.6f;
        mr.DrawLine(transform.TransformPoint(new Vector3(sa, -0.3f, 0)),
                    transform.TransformPoint(new Vector3(sa, F(aVal) * 0.5f + 0.2f, 0)),
                    new Color(0.3f, 0.8f, 1f, 0.5f), 0.005f);
        mr.DrawLine(transform.TransformPoint(new Vector3(sb, -0.3f, 0)),
                    transform.TransformPoint(new Vector3(sb, F(bVal) * 0.5f + 0.2f, 0)),
                    new Color(1f, 0.5f, 0.3f, 0.5f), 0.005f);

        float area = FAnalytic(bVal) - FAnalytic(aVal);
        areaLabel.text = $"∫ f(x)dx = {area:F3}";
        areaLabel.color = area > 5f ? new Color(0.3f, 1f, 0.5f) : Color.white;
        boundsLabel.text = $"a = {aVal:F2}    b = {bVal:F2}    F(b)-F(a) = {area:F3}";
    }

    public override bool CheckChallengeComplete()
    {
        float aVal = Mathf.Lerp(-3f, 0, Mathf.InverseLerp(-2.5f, 0, handleA.LocalPosition.x));
        float bVal = Mathf.Lerp(0, 3f, Mathf.InverseLerp(0, 2.5f, handleB.LocalPosition.x));
        float area = FAnalytic(bVal) - FAnalytic(aVal);
        return area > 5f;
    }
}
