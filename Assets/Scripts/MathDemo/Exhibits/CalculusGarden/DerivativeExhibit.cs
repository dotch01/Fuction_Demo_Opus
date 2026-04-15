using UnityEngine;

// ============================================================
// DerivativeExhibit.cs — 導數微分
// 曲線上的切線斜率 + ε 趨近動畫
// ============================================================

public class DerivativeExhibit : ExhibitBase
{
    private DragHandle handleX, handleEps;
    private TextMesh derivLabel, slopeLabel, epsLabel;

    public override void BuildExhibit()
    {
        exhibitName = "導數微分 Derivative";
        description = "導數 = 瞬間變化率 = 切線斜率\n\nf'(x) = lim(ε→0) [f(x+ε) - f(x)] / ε\n\nε 越小 → 割線越接近切線\n\n🎮 遊戲應用：\n• 速度 = 位置的導數 = 每秒移動量\n• 加速度 = 速度的導數\n• 地形坡度 = 高度函數的導數\n• 動畫曲線的切線就是變化率\n\n紅線=割線  綠線=切線\n拖曳控制 x 和 ε";
        formula = "f(x) = sin(x)    f'(x) = cos(x)";
        challengeDescription = "讓 ε < 0.1 看到割線趨近切線";

        handleX = CreateDragHandle(new Vector3(0, -2f, 0), new Color(0.3f, 0.8f, 1f), 0.12f);
        handleX.minBounds = new Vector3(-2.5f, -2f, 0);
        handleX.maxBounds = new Vector3(2.5f, -2f, 0);

        handleEps = CreateDragHandle(new Vector3(0, -3f, 0), new Color(1f, 0.85f, 0.2f), 0.12f);
        handleEps.minBounds = new Vector3(-2.5f, -3f, 0);
        handleEps.maxBounds = new Vector3(2.5f, -3f, 0);

        CreateLabel(new Vector3(0, -1.5f, 0), "x 位置", 18, new Color(0.4f, 0.4f, 0.5f));
        CreateLabel(new Vector3(0, -2.5f, 0), "ε 大小", 18, new Color(0.4f, 0.4f, 0.5f));

        derivLabel = CreateLabel(new Vector3(0, 2.5f, 0), "", 28, Color.white);
        slopeLabel = CreateLabel(new Vector3(0, -3.6f, 0), "", 26, new Color(0.7f, 0.8f, 1f));
        epsLabel = CreateLabel(new Vector3(0, -4.2f, 0), "", 24, new Color(0.7f, 0.7f, 0.7f));
    }

    private float F(float x) => Mathf.Sin(x * 1.5f);
    private float FP(float x) => 1.5f * Mathf.Cos(x * 1.5f);

    public override void UpdateVisualization()
    {
        var mr = MathLineRenderer.Instance;
        if (mr == null) return;

        float xVal = Mathf.Lerp(-3f, 3f, Mathf.InverseLerp(-2.5f, 2.5f, handleX.LocalPosition.x));
        float eps = Mathf.Lerp(0.02f, 2f, Mathf.InverseLerp(-2.5f, 2.5f, handleEps.LocalPosition.x));

        // 畫曲線
        int steps = 60;
        for (int i = 0; i < steps; i++)
        {
            float x0 = Mathf.Lerp(-4f, 4f, (float)i / steps);
            float x1 = Mathf.Lerp(-4f, 4f, (float)(i + 1) / steps);
            mr.DrawLine(transform.TransformPoint(new Vector3(x0 * 0.5f, F(x0) * 0.6f, 0)),
                        transform.TransformPoint(new Vector3(x1 * 0.5f, F(x1) * 0.6f, 0)),
                        new Color(0.3f, 0.5f, 0.8f), 0.01f);
        }

        float y = F(xVal);
        float ye = F(xVal + eps);
        float secantSlope = (ye - y) / eps;
        float tangentSlope = FP(xVal);

        // 割線（紅）
        float secantLen = 1.5f;
        Vector3 secStart = new Vector3((xVal - secantLen) * 0.5f, (y - secantSlope * secantLen) * 0.6f, 0);
        Vector3 secEnd = new Vector3((xVal + secantLen) * 0.5f, (y + secantSlope * secantLen) * 0.6f, 0);
        mr.DrawLine(transform.TransformPoint(secStart), transform.TransformPoint(secEnd),
            new Color(1f, 0.3f, 0.3f), 0.01f);

        // 切線（綠）
        float tanLen = 1.5f;
        Vector3 tanStart = new Vector3((xVal - tanLen) * 0.5f, (y - tangentSlope * tanLen) * 0.6f, 0);
        Vector3 tanEnd = new Vector3((xVal + tanLen) * 0.5f, (y + tangentSlope * tanLen) * 0.6f, 0);
        mr.DrawLine(transform.TransformPoint(tanStart), transform.TransformPoint(tanEnd),
            new Color(0.3f, 1f, 0.5f), 0.012f);

        // 點標記
        Vector3 ptW = transform.TransformPoint(new Vector3(xVal * 0.5f, y * 0.6f, 0));
        mr.DrawLine(ptW - Vector3.right * 0.03f, ptW + Vector3.right * 0.03f, Color.yellow, 0.05f);

        derivLabel.text = $"f({xVal:F2}) = {y:F3}    f'({xVal:F2}) = {tangentSlope:F3}";
        slopeLabel.text = $"割線斜率 = {secantSlope:F3}    切線斜率 = {tangentSlope:F3}";
        epsLabel.text = $"ε = {eps:F3}    誤差 = {Mathf.Abs(secantSlope - tangentSlope):F4}";
    }

    public override bool CheckChallengeComplete()
    {
        float eps = Mathf.Lerp(0.02f, 2f, Mathf.InverseLerp(-2.5f, 2.5f, handleEps.LocalPosition.x));
        return eps < 0.1f;
    }
}
