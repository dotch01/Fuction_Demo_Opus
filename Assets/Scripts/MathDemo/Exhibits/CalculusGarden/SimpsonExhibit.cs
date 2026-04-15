using UnityEngine;

// ============================================================
// SimpsonExhibit.cs — 辛普森規則
// 調函數和分段數，看面積逼近
// ============================================================

public class SimpsonExhibit : ExhibitBase
{
    private DragHandle handleN;
    private TextMesh resultLabel, exactLabel, errorLabel;

    public override void BuildExhibit()
    {
        exhibitName = "辛普森規則 Simpson";
        description = "數值積分：用拋物線逼近曲線！\n\n∫f(x)dx ≈ (h/3)[f₀ + 4f₁ + 2f₂ + ... + fₙ]\n\n比矩形法精確得多（O(h⁴) 誤差）\n\n🎮 遊戲應用：\n• 物理引擎的力積分（RK4 類似思想）\n• 弧長計算（曲線路徑總長）\n• 音訊處理的數值分析\n• 面積/體積的程式計算\n\n拖曳控制分段數 n\nn 越大 → 越精確";
        formula = "S = (h/3)[f₀+4f₁+2f₂+...+fₙ]    精確值 = 2";
        challengeDescription = "讓 n ≥ 8 使誤差 < 0.001";

        handleN = CreateDragHandle(new Vector3(0, -2.5f, 0), new Color(0.3f, 0.8f, 1f), 0.12f);
        handleN.minBounds = new Vector3(-2.5f, -2.5f, 0);
        handleN.maxBounds = new Vector3(2.5f, -2.5f, 0);

        CreateLabel(new Vector3(-2.8f, -2.5f, 0), "n=2", 20, new Color(0.5f, 0.5f, 0.6f));
        CreateLabel(new Vector3(2.8f, -2.5f, 0), "n=20", 20, new Color(0.5f, 0.5f, 0.6f));
        CreateLabel(new Vector3(0, 2.2f, 0), "∫₀π sin(x)dx = 2", 24, new Color(0.6f, 0.6f, 0.7f));

        resultLabel = CreateLabel(new Vector3(0, -1.5f, 0), "", 30, Color.white);
        exactLabel = CreateLabel(new Vector3(0, -3.2f, 0), "", 26, new Color(0.7f, 0.8f, 1f));
        errorLabel = CreateLabel(new Vector3(0, -3.8f, 0), "", 24, Color.white);
    }

    public override void UpdateVisualization()
    {
        var mr = MathLineRenderer.Instance;
        if (mr == null) return;

        float t = Mathf.InverseLerp(-2.5f, 2.5f, handleN.LocalPosition.x);
        int n = Mathf.Max(2, Mathf.RoundToInt(Mathf.Lerp(2, 20, t)));
        if (n % 2 != 0) n++; // Simpson 需偶數

        float a = 0, b = Mathf.PI;
        float h = (b - a) / n;

        // 畫曲線
        int steps = 50;
        for (int i = 0; i < steps; i++)
        {
            float x0 = Mathf.Lerp(a, b, (float)i / steps);
            float x1 = Mathf.Lerp(a, b, (float)(i + 1) / steps);
            mr.DrawLine(transform.TransformPoint(new Vector3((x0 / Mathf.PI) * 4f - 2f, Mathf.Sin(x0) * 1.5f, 0)),
                        transform.TransformPoint(new Vector3((x1 / Mathf.PI) * 4f - 2f, Mathf.Sin(x1) * 1.5f, 0)),
                        new Color(0.3f, 0.5f, 0.8f), 0.01f);
        }

        // 畫辛普森分段拋物線
        float simpsonResult = 0;
        for (int i = 0; i < n; i += 2)
        {
            float x0 = a + i * h;
            float x1 = a + (i + 1) * h;
            float x2 = a + (i + 2) * h;
            float f0 = Mathf.Sin(x0), f1 = Mathf.Sin(x1), f2 = Mathf.Sin(x2);
            simpsonResult += (h / 3f) * (f0 + 4 * f1 + f2);

            // 畫分段
            int subSteps = 8;
            for (int j = 0; j < subSteps; j++)
            {
                float ta = (float)j / subSteps;
                float tb = (float)(j + 1) / subSteps;
                float xa = Mathf.Lerp(x0, x2, ta);
                float xb = Mathf.Lerp(x0, x2, tb);

                // 拋物線插值
                float ya = ParabolicInterp(x0, f0, x1, f1, x2, f2, xa);
                float yb = ParabolicInterp(x0, f0, x1, f1, x2, f2, xb);

                float screenXA = (xa / Mathf.PI) * 4f - 2f;
                float screenXB = (xb / Mathf.PI) * 4f - 2f;

                // 填充
                mr.DrawLine(transform.TransformPoint(new Vector3(screenXA, 0, 0)),
                            transform.TransformPoint(new Vector3(screenXA, ya * 1.5f, 0)),
                            new Color(0.3f, 0.8f, 0.5f, 0.3f), 0.008f);
            }

            // 分段邊界
            float sx = (x0 / Mathf.PI) * 4f - 2f;
            mr.DrawLine(transform.TransformPoint(new Vector3(sx, 0, 0)),
                        transform.TransformPoint(new Vector3(sx, f0 * 1.5f, 0)),
                        new Color(0.5f, 0.5f, 0.6f), 0.005f);
        }

        float exact = 2f;
        float error = Mathf.Abs(simpsonResult - exact);

        resultLabel.text = $"Simpson(n={n}) = {simpsonResult:F6}";
        exactLabel.text = $"精確值 = {exact:F6}";
        errorLabel.text = $"誤差 = {error:F6}";
        errorLabel.color = error < 0.001f ? new Color(0.3f, 1f, 0.5f) : new Color(1f, 0.5f, 0.3f);
    }

    private float ParabolicInterp(float x0, float f0, float x1, float f1, float x2, float f2, float x)
    {
        float l0 = ((x - x1) * (x - x2)) / ((x0 - x1) * (x0 - x2));
        float l1 = ((x - x0) * (x - x2)) / ((x1 - x0) * (x1 - x2));
        float l2 = ((x - x0) * (x - x1)) / ((x2 - x0) * (x2 - x1));
        return f0 * l0 + f1 * l1 + f2 * l2;
    }

    public override bool CheckChallengeComplete()
    {
        float t = Mathf.InverseLerp(-2.5f, 2.5f, handleN.LocalPosition.x);
        int n = Mathf.Max(2, Mathf.RoundToInt(Mathf.Lerp(2, 20, t)));
        if (n % 2 != 0) n++;
        return n >= 8;
    }
}
