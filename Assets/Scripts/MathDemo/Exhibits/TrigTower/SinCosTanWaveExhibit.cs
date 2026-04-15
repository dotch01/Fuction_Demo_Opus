using UnityEngine;

// ============================================================
// SinCosTanWaveExhibit.cs — sin cos tan 波形
// 疊合波形 + 振幅/頻率/相位滑桿
// ============================================================

public class SinCosTanWaveExhibit : ExhibitBase
{
    private DragHandle handleFreq, handleAmp;
    private TextMesh freqLabel, ampLabel, infoLabel;

    public override void BuildExhibit()
    {
        exhibitName = "sin cos tan 波形";
        description = "三角函數波形：\n\ny = A · sin(ωx + φ)\n\n• A = 振幅（高度）\n• ω = 角頻率（密度）\n• φ = 相位（偏移）\n\n🎮 遊戲應用：\n• 水面波浪起伏 = 多個 sin 疊加\n• 怪物上下飄浮動畫\n• 風吹草動效果\n• 音效波形合成\n\n拖曳控制振幅和頻率";
        formula = "y = A·sin(ωx)    cos(x) = sin(x+π/2)    tan = sin/cos";
        challengeDescription = "讓頻率 > 3 且振幅 > 1.5";

        handleAmp = CreateDragHandle(new Vector3(-2.5f, 0, 0), new Color(1f, 0.5f, 0.3f), 0.12f);
        handleAmp.minBounds = new Vector3(-2.5f, -2f, 0);
        handleAmp.maxBounds = new Vector3(-2.5f, 2f, 0);

        handleFreq = CreateDragHandle(new Vector3(2.5f, 0, 0), new Color(0.3f, 0.7f, 1f), 0.12f);
        handleFreq.minBounds = new Vector3(2.5f, -2f, 0);
        handleFreq.maxBounds = new Vector3(2.5f, 2f, 0);

        CreateLabel(new Vector3(-2.5f, -2.5f, 0), "振幅 A", 20, new Color(0.5f, 0.5f, 0.6f));
        CreateLabel(new Vector3(2.5f, -2.5f, 0), "頻率 ω", 20, new Color(0.5f, 0.5f, 0.6f));

        freqLabel = CreateLabel(new Vector3(0, -2.8f, 0), "", 28, Color.white);
        ampLabel = CreateLabel(new Vector3(0, -3.4f, 0), "", 28, Color.white);
        infoLabel = CreateLabel(new Vector3(0, 2.5f, 0), "紅=sin  綠=cos  黃=tan", 24, new Color(0.6f, 0.6f, 0.7f));
    }

    public override void UpdateVisualization()
    {
        var mr = MathLineRenderer.Instance;
        if (mr == null) return;

        float amp = Mathf.Lerp(0.2f, 2.5f, Mathf.InverseLerp(-2f, 2f, handleAmp.LocalPosition.y));
        float freq = Mathf.Lerp(0.5f, 5f, Mathf.InverseLerp(-2f, 2f, handleFreq.LocalPosition.y));

        int steps = 80;
        float xRange = 4f;

        for (int i = 0; i < steps; i++)
        {
            float x0 = Mathf.Lerp(-xRange, xRange, (float)i / steps);
            float x1 = Mathf.Lerp(-xRange, xRange, (float)(i + 1) / steps);

            // sin
            float y0s = amp * Mathf.Sin(freq * x0);
            float y1s = amp * Mathf.Sin(freq * x1);
            mr.DrawLine(transform.TransformPoint(new Vector3(x0 * 0.5f, y0s * 0.5f, 0)),
                        transform.TransformPoint(new Vector3(x1 * 0.5f, y1s * 0.5f, 0)),
                        new Color(1f, 0.3f, 0.3f), 0.012f);

            // cos
            float y0c = amp * Mathf.Cos(freq * x0);
            float y1c = amp * Mathf.Cos(freq * x1);
            mr.DrawLine(transform.TransformPoint(new Vector3(x0 * 0.5f, y0c * 0.5f, 0)),
                        transform.TransformPoint(new Vector3(x1 * 0.5f, y1c * 0.5f, 0)),
                        new Color(0.3f, 0.9f, 0.4f), 0.012f);

            // tan (clamped)
            float y0t = Mathf.Clamp(amp * Mathf.Tan(freq * x0), -3f, 3f);
            float y1t = Mathf.Clamp(amp * Mathf.Tan(freq * x1), -3f, 3f);
            if (Mathf.Abs(y0t - y1t) < 4f) // 跳過漸近線
            {
                mr.DrawLine(transform.TransformPoint(new Vector3(x0 * 0.5f, y0t * 0.3f, 0)),
                            transform.TransformPoint(new Vector3(x1 * 0.5f, y1t * 0.3f, 0)),
                            new Color(1f, 0.85f, 0.2f), 0.008f);
            }
        }

        freqLabel.text = $"頻率 ω = {freq:F2}";
        ampLabel.text = $"振幅 A = {amp:F2}";
    }

    public override bool CheckChallengeComplete()
    {
        float amp = Mathf.Lerp(0.2f, 2.5f, Mathf.InverseLerp(-2f, 2f, handleAmp.LocalPosition.y));
        float freq = Mathf.Lerp(0.5f, 5f, Mathf.InverseLerp(-2f, 2f, handleFreq.LocalPosition.y));
        return freq > 3f && amp > 1.5f;
    }
}
