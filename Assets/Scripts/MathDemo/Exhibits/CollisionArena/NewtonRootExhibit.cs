using UnityEngine;
using System.Collections.Generic;

// ============================================================
// NewtonRootExhibit.cs — 牛頓法求根
// 拋物線碰地面的迭代逼近
// ============================================================

public class NewtonRootExhibit : ExhibitBase
{
    private DragHandle handleX0;
    private TextMesh iterLabel, resultLabel, infoLabel;
    private float a = 1f, b = -2f, c = -1f; // f(x) = x² - 2x - 1

    public override void BuildExhibit()
    {
        exhibitName = "牛頓法 Newton's Method";
        description = "找 f(x) = 0 的根！\n\nf(x) = x² - 2x - 1\n\n迭代公式：\nx_{n+1} = x_n - f(x_n) / f'(x_n)\n\n收斂極快（二次收斂 → 幾步就到）\n\n🎮 遊戲應用：\n• IK 逆運動學求解器\n• 物理約束求解\n• 拋物線軋跡預測著地點\n• Shader 中的 SDF Ray Marching\n\n拖曳初始猜測 x₀";
        formula = "x_{n+1} = x_n - f(x_n)/f'(x_n)";
        challengeDescription = "讓牛頓法在 5 步內收斂到 |f(x)| < 0.01";

        // 畫曲線的範圍
        handleX0 = CreateDragHandle(new Vector3(-1.5f, -1.5f, 0), new Color(1f, 0.5f, 0.3f), 0.12f);
        handleX0.minBounds = new Vector3(-2.5f, -1.5f, 0);
        handleX0.maxBounds = new Vector3(2.5f, -1.5f, 0);

        CreateLabel(new Vector3(0, 2.5f, 0), "f(x) = x² - 2x - 1", 28, new Color(0.6f, 0.6f, 0.7f));
        iterLabel = CreateLabel(new Vector3(0, -2.3f, 0), "", 26, Color.white);
        resultLabel = CreateLabel(new Vector3(0, -2.9f, 0), "", 28, Color.white);
        infoLabel = CreateLabel(new Vector3(0, -3.5f, 0), "", 22, new Color(0.7f, 0.7f, 0.8f));
    }

    private float F(float x) => a * x * x + b * x + c;
    private float FP(float x) => 2 * a * x + b;

    public override void UpdateVisualization()
    {
        var mr = MathLineRenderer.Instance;
        if (mr == null) return;

        // 畫曲線
        int steps = 60;
        for (int i = 0; i < steps; i++)
        {
            float x0 = Mathf.Lerp(-3f, 4f, (float)i / steps);
            float x1 = Mathf.Lerp(-3f, 4f, (float)(i + 1) / steps);
            float y0 = F(x0), y1 = F(x1);
            mr.DrawLine(transform.TransformPoint(new Vector3(x0 * 0.5f, y0 * 0.3f, 0)),
                        transform.TransformPoint(new Vector3(x1 * 0.5f, y1 * 0.3f, 0)),
                        new Color(0.3f, 0.5f, 0.8f), 0.01f);
        }

        // X 軸
        mr.DrawLine(transform.TransformPoint(new Vector3(-2f, 0, 0)), transform.TransformPoint(new Vector3(2.5f, 0, 0)),
            new Color(0.4f, 0.4f, 0.5f), 0.005f);

        // 牛頓迭代
        float startX = Mathf.Lerp(-3f, 5f, Mathf.InverseLerp(-2.5f, 2.5f, handleX0.LocalPosition.x));
        float x = startX;
        var iterPoints = new List<Vector2>();
        iterPoints.Add(new Vector2(x, F(x)));

        string iterText = $"x₀={x:F2}";
        for (int i = 0; i < 6; i++)
        {
            float fx = F(x);
            float fpx = FP(x);
            if (Mathf.Abs(fpx) < 0.0001f) break;

            // 畫切線
            float xNew = x - fx / fpx;
            Vector3 pCurve = transform.TransformPoint(new Vector3(x * 0.5f, fx * 0.3f, 0));
            Vector3 pAxis = transform.TransformPoint(new Vector3(xNew * 0.5f, 0, 0));
            mr.DrawLine(pCurve, pAxis, new Color(1f, 0.85f, 0.2f, 0.6f), 0.008f);

            // 垂直線到曲線
            float fxNew = F(xNew);
            Vector3 pNewCurve = transform.TransformPoint(new Vector3(xNew * 0.5f, fxNew * 0.3f, 0));
            mr.DrawLine(pAxis, pNewCurve, new Color(0.5f, 0.5f, 0.6f, 0.4f), 0.005f);

            x = xNew;
            iterText += $" → {x:F3}";
            if (Mathf.Abs(F(x)) < 0.01f) break;
        }

        iterLabel.text = iterText;
        resultLabel.text = $"f({x:F4}) = {F(x):F6}";
        resultLabel.color = Mathf.Abs(F(x)) < 0.01f ? new Color(0.3f, 1f, 0.5f) : new Color(1f, 0.5f, 0.3f);
        infoLabel.text = "每步沿切線找零點 → 二次收斂";
    }

    public override bool CheckChallengeComplete()
    {
        float x = Mathf.Lerp(-3f, 5f, Mathf.InverseLerp(-2.5f, 2.5f, handleX0.LocalPosition.x));
        for (int i = 0; i < 5; i++)
        {
            float fpx = FP(x);
            if (Mathf.Abs(fpx) < 0.0001f) return false;
            x = x - F(x) / fpx;
        }
        return Mathf.Abs(F(x)) < 0.01f;
    }
}
