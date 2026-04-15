using UnityEngine;

// ============================================================
// EulerIdentityExhibit.cs — 歐拉恆等式
// e^(iθ) = cosθ + i·sinθ 的視覺化
// 複數平面上旋轉
// ============================================================

public class EulerIdentityExhibit : ExhibitBase
{
    private DragHandle handleAngle;
    private TextMesh eulerLabel;
    private TextMesh complexLabel;
    private float animAngle;

    public override void BuildExhibit()
    {
        exhibitName = "歐拉恆等式 Euler's Formula";
        description = "e^(iθ) = cosθ + i·sinθ\n\n這是數學中最美的公式：\n• 在複數平面上，e^(iθ) 描述一個旋轉\n• 實部 = cosθ（水平）\n• 虛部 = sinθ（垂直）\n• θ = π 時：e^(iπ) + 1 = 0\n\n🎮 遊戲應用：\n• 旋轉的數學本質（Quaternion 的 2D 版）\n• 音訊 FFT 頻譜分析的基礎\n• 波形干涉模擬\n\n拖曳改變 θ，觀察旋轉";
        formula = "e^(iπ) + 1 = 0";
        challengeDescription = "讓 θ = π（180°），呈現 e^(iπ) = -1";

        // 複數平面軸
        CreateStaticPrimitive(PrimitiveType.Cube,
            Vector3.zero, new Vector3(5.5f, 0.03f, 0.03f), new Color(0.4f, 0.4f, 0.4f));
        CreateStaticPrimitive(PrimitiveType.Cube,
            Vector3.zero, new Vector3(0.03f, 5.5f, 0.03f), new Color(0.4f, 0.4f, 0.4f));

        CreateLabel(new Vector3(2.8f, -0.3f, 0), "實部 Re", 22, new Color(0.6f, 0.6f, 0.6f));
        CreateLabel(new Vector3(0.3f, 2.8f, 0), "虛部 Im", 22, new Color(0.6f, 0.6f, 0.6f));

        // 角度控制滑桿
        handleAngle = CreateDragHandle(new Vector3(0, -3f, 0), new Color(1f, 0.85f, 0.3f), 0.13f);
        handleAngle.minBounds = new Vector3(-3.14f, -3f, 0);
        handleAngle.maxBounds = new Vector3(3.14f, -3f, 0);

        CreateLabel(new Vector3(-3.14f, -2.6f, 0), "-π", 22, new Color(0.5f, 0.5f, 0.5f));
        CreateLabel(new Vector3(0, -2.6f, 0), "0", 22, new Color(0.5f, 0.5f, 0.5f));
        CreateLabel(new Vector3(3.14f, -2.6f, 0), "π", 22, new Color(0.5f, 0.5f, 0.5f));

        eulerLabel = CreateLabel(new Vector3(0, -3.8f, 0), "", 34, new Color(1f, 0.9f, 0.4f));
        complexLabel = CreateLabel(new Vector3(0, -4.4f, 0), "", 30, Color.white);
    }

    public override void UpdateVisualization()
    {
        if (MathLineRenderer.Instance == null) return;
        var mr = MathLineRenderer.Instance;

        float theta = handleAngle.LocalPosition.x; // 直接用 x 當弧度
        float r = 2f;

        Vector3 origin = transform.position;

        float cosT = Mathf.Cos(theta);
        float sinT = Mathf.Sin(theta);
        Vector3 point = new Vector3(cosT * r, sinT * r, 0);
        Vector3 worldPoint = transform.TransformPoint(point);

        // 單位圓
        mr.DrawCircle(origin, r, Vector3.forward, new Color(0.35f, 0.35f, 0.4f), 0.01f);

        // e^(iθ) 點到原點的線
        mr.DrawLine(origin, worldPoint, new Color(1f, 0.85f, 0.3f), 0.03f);
        mr.DrawPoint(worldPoint, new Color(1f, 0.85f, 0.3f), 0.12f);

        // 實部和虛部分量
        Vector3 realEnd = transform.TransformPoint(new Vector3(cosT * r, 0, 0));
        mr.DrawLine(origin, realEnd, new Color(0.3f, 0.7f, 1f), 0.025f);         // 實軸
        mr.DrawLine(realEnd, worldPoint, new Color(1f, 0.4f, 0.7f), 0.025f);      // 虛軸

        // 角度弧
        float angleDeg = theta * Mathf.Rad2Deg;
        mr.DrawArc(origin, Vector3.right, angleDeg, 0.7f, new Color(1f, 0.85f, 0.3f, 0.5f), 0.015f);

        // 螺旋軌跡（展示多個角度）
        for (int i = 0; i < 30; i++)
        {
            float t0 = (float)i / 30 * theta;
            float t1 = (float)(i + 1) / 30 * theta;
            Vector3 a = new Vector3(Mathf.Cos(t0) * r, Mathf.Sin(t0) * r, 0);
            Vector3 b = new Vector3(Mathf.Cos(t1) * r, Mathf.Sin(t1) * r, 0);
            mr.DrawLine(transform.TransformPoint(a), transform.TransformPoint(b),
                        Color.Lerp(new Color(0.3f, 0.7f, 1f), new Color(1f, 0.4f, 0.7f), (float)i / 30), 0.02f);
        }

        // 滑桿背景
        mr.DrawLine(transform.TransformPoint(new Vector3(-3.14f, -3f, 0)),
                    transform.TransformPoint(new Vector3(3.14f, -3f, 0)),
                    new Color(0.3f, 0.3f, 0.3f), 0.008f);

        // 特殊標記
        bool isPi = Mathf.Abs(theta - Mathf.PI) < 0.1f;
        bool isNegOne = isPi;

        eulerLabel.text = $"e^(i × {theta:F2}) = {cosT:F3} + {sinT:F3}i";
        complexLabel.text = isPi ?
            "★ e^(iπ) + 1 = 0  歐拉恆等式！" :
            $"θ = {angleDeg:F1}°    |e^(iθ)| = 1（永遠在單位圓上）";

        if (isPi) eulerLabel.color = new Color(0.3f, 1f, 0.5f);
        else eulerLabel.color = new Color(1f, 0.9f, 0.4f);
    }

    public override bool CheckChallengeComplete()
    {
        float theta = handleAngle.LocalPosition.x;
        return Mathf.Abs(theta - Mathf.PI) < 0.08f;
    }

    protected override void OnChallengeCompleted()
    {
        base.OnChallengeCompleted();
        ChallengeSystem.Instance?.MarkCompleted(exhibitName);
    }
}
