using UnityEngine;

// ============================================================
// WaveSuperpositionExhibit.cs — 波疊加
// 多個 sin 波相加的干涉效果
// ============================================================

public class WaveSuperpositionExhibit : ExhibitBase
{
    private DragHandle handleFreq2, handlePhase;
    private TextMesh sumLabel, infoLabel;

    public override void BuildExhibit()
    {
        exhibitName = "波疊加 Wave Superposition";
        description = "兩個波的疊加：\n\ny = sin(ω₁x) + sin(ω₂x + φ)\n\n• ω₁ = ω₂ → 建設性干涉（振幅加倍）\n• 相位差 π → 破壞性干涉（抵消）\n\n🎮 遊戲應用：\n• 海洋 Shader：多個 sin 波疊加 = 真實水面\n• 音效合成：不同頻率混合出音色\n• 干涉花紋：魔法陣/能量場效果\n• 無線電干擾/雜訊模擬\n\n拖曳控制第二波的頻率和相位";
        formula = "y = sin(ω₁x) + sin(ω₂x+φ)    拍頻 = |ω₁-ω₂|";
        challengeDescription = "讓兩波完全抵消（破壞性干涉）";

        handleFreq2 = CreateDragHandle(new Vector3(-2.5f, -1.5f, 0), new Color(1f, 0.5f, 0.3f), 0.12f);
        handleFreq2.minBounds = new Vector3(-2.5f, -2.5f, 0);
        handleFreq2.maxBounds = new Vector3(-2.5f, -0.5f, 0);

        handlePhase = CreateDragHandle(new Vector3(2.5f, -1.5f, 0), new Color(0.5f, 0.8f, 1f), 0.12f);
        handlePhase.minBounds = new Vector3(2.5f, -2.5f, 0);
        handlePhase.maxBounds = new Vector3(2.5f, -0.5f, 0);

        CreateLabel(new Vector3(-2.5f, -3f, 0), "ω₂", 20, new Color(0.5f, 0.5f, 0.6f));
        CreateLabel(new Vector3(2.5f, -3f, 0), "φ", 20, new Color(0.5f, 0.5f, 0.6f));

        sumLabel = CreateLabel(new Vector3(0, -1.5f, 0), "", 26, Color.white);
        infoLabel = CreateLabel(new Vector3(0, -3.5f, 0), "", 22, new Color(0.7f, 0.7f, 0.8f));
    }

    public override void UpdateVisualization()
    {
        var mr = MathLineRenderer.Instance;
        if (mr == null) return;

        float omega1 = 2f;
        float omega2 = Mathf.Lerp(1f, 4f, Mathf.InverseLerp(-2.5f, -0.5f, handleFreq2.LocalPosition.y));
        float phase = Mathf.Lerp(0, Mathf.PI * 2f, Mathf.InverseLerp(-2.5f, -0.5f, handlePhase.LocalPosition.y));

        int steps = 80;
        float xRange = 4f;
        float yScale = 0.4f;

        for (int i = 0; i < steps; i++)
        {
            float x0 = Mathf.Lerp(-xRange, xRange, (float)i / steps);
            float x1 = Mathf.Lerp(-xRange, xRange, (float)(i + 1) / steps);

            // Wave 1 (blue)
            float y0a = Mathf.Sin(omega1 * x0);
            float y1a = Mathf.Sin(omega1 * x1);
            mr.DrawLine(transform.TransformPoint(new Vector3(x0 * 0.5f, y0a * yScale + 0.8f, 0)),
                        transform.TransformPoint(new Vector3(x1 * 0.5f, y1a * yScale + 0.8f, 0)),
                        new Color(0.3f, 0.5f, 0.9f, 0.6f), 0.006f);

            // Wave 2 (red)
            float y0b = Mathf.Sin(omega2 * x0 + phase);
            float y1b = Mathf.Sin(omega2 * x1 + phase);
            mr.DrawLine(transform.TransformPoint(new Vector3(x0 * 0.5f, y0b * yScale + 0.3f, 0)),
                        transform.TransformPoint(new Vector3(x1 * 0.5f, y1b * yScale + 0.3f, 0)),
                        new Color(0.9f, 0.4f, 0.3f, 0.6f), 0.006f);

            // Sum (yellow, thicker)
            float y0s = y0a + y0b;
            float y1s = y1a + y1b;
            mr.DrawLine(transform.TransformPoint(new Vector3(x0 * 0.5f, y0s * yScale * 0.5f - 0.5f, 0)),
                        transform.TransformPoint(new Vector3(x1 * 0.5f, y1s * yScale * 0.5f - 0.5f, 0)),
                        new Color(1f, 0.85f, 0.2f), 0.012f);
        }

        float phaseDeg = phase * Mathf.Rad2Deg;
        string interference;
        if (Mathf.Abs(omega1 - omega2) < 0.1f && Mathf.Abs(Mathf.Sin(phase / 2f)) < 0.15f)
            interference = "建設性干涉！振幅加倍";
        else if (Mathf.Abs(omega1 - omega2) < 0.1f && Mathf.Abs(Mathf.Cos(phase / 2f)) < 0.15f)
            interference = "破壞性干涉！完全抵消";
        else
            interference = $"拍頻 = |{omega1:F1}-{omega2:F1}| = {Mathf.Abs(omega1 - omega2):F2}";

        sumLabel.text = $"ω₁={omega1:F1} ω₂={omega2:F1} φ={phaseDeg:F0}°";
        infoLabel.text = interference;
    }

    public override bool CheckChallengeComplete()
    {
        float omega2 = Mathf.Lerp(1f, 4f, Mathf.InverseLerp(-2.5f, -0.5f, handleFreq2.LocalPosition.y));
        float phase = Mathf.Lerp(0, Mathf.PI * 2f, Mathf.InverseLerp(-2.5f, -0.5f, handlePhase.LocalPosition.y));
        return Mathf.Abs(2f - omega2) < 0.2f && Mathf.Abs(Mathf.Cos(phase / 2f)) < 0.2f;
    }
}
