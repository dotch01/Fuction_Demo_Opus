using UnityEngine;

// ============================================================
// UnitCircleExhibit.cs — 單位圓與三角函數
// 拖曳角度點，即時顯示 sin/cos/tan 值和對應線段
// ============================================================

public class UnitCircleExhibit : ExhibitBase
{
    private DragHandle handleAngle;
    private TextMesh sinLabel, cosLabel, tanLabel, angleLabel;

    private float circleRadius = 2f;

    public override void BuildExhibit()
    {
        exhibitName = "單位圓 Unit Circle";
        description = "圓上任一點 P 的座標 = (cosθ, sinθ)\n\n• 紅色水平線 = cosθ\n• 綠色垂直線 = sinθ\n• 黃色切線 = tanθ = sinθ/cosθ\n\n🎮 遊戲應用：\n• 圓形移動：x=cos(t), y=sin(t)\n• 雷達掃描線旋轉\n• 行星軋道/衛星環繞\n• 環形 UI 選單的物件排列\n\n拖曳圓上的點改變角度";
        formula = "P = (cosθ, sinθ)    tanθ = sinθ/cosθ";
        challengeDescription = "讓角度精準停在 45°（±3°）";

        // 座標軸
        CreateStaticPrimitive(PrimitiveType.Cube,
            Vector3.zero, new Vector3(5f, 0.03f, 0.03f), new Color(0.4f, 0.4f, 0.4f));
        CreateStaticPrimitive(PrimitiveType.Cube,
            Vector3.zero, new Vector3(0.03f, 5f, 0.03f), new Color(0.4f, 0.4f, 0.4f));

        // 拖曳點（初始在 45°）
        float initAngle = 45f * Mathf.Deg2Rad;
        handleAngle = CreateDragHandle(
            new Vector3(Mathf.Cos(initAngle) * circleRadius, Mathf.Sin(initAngle) * circleRadius, 0),
            new Color(1f, 1f, 1f), 0.18f);
        handleAngle.minBounds = new Vector3(-3f, -3f, 0);
        handleAngle.maxBounds = new Vector3(3f, 3f, 0);

        // 數值標籤
        cosLabel = CreateLabel(new Vector3(0, -3f, 0), "", 32, new Color(1f, 0.4f, 0.4f));
        sinLabel = CreateLabel(new Vector3(0, -3.5f, 0), "", 32, new Color(0.4f, 0.9f, 0.4f));
        tanLabel = CreateLabel(new Vector3(0, -4f, 0), "", 30, new Color(1f, 0.9f, 0.3f));
        angleLabel = CreateLabel(new Vector3(1.5f, 2.8f, 0), "", 30, Color.white);

        // 特殊角度標記
        CreateLabel(new Vector3(circleRadius + 0.3f, -0.2f, 0), "0°", 20, new Color(0.5f, 0.5f, 0.5f));
        CreateLabel(new Vector3(-0.5f, circleRadius + 0.2f, 0), "90°", 20, new Color(0.5f, 0.5f, 0.5f));
        CreateLabel(new Vector3(-circleRadius - 0.6f, -0.2f, 0), "180°", 20, new Color(0.5f, 0.5f, 0.5f));
        CreateLabel(new Vector3(-0.5f, -circleRadius - 0.4f, 0), "270°", 20, new Color(0.5f, 0.5f, 0.5f));
    }

    public override void UpdateVisualization()
    {
        if (MathLineRenderer.Instance == null) return;
        var mr = MathLineRenderer.Instance;

        Vector3 origin = transform.position;
        Vector3 handlePos = handleAngle.LocalPosition;

        // 將拖曳位置限制到圓上
        float angle = Mathf.Atan2(handlePos.y, handlePos.x);
        Vector3 pointOnCircle = new Vector3(Mathf.Cos(angle) * circleRadius, Mathf.Sin(angle) * circleRadius, 0);

        // 強制拖曳點到圓上
        handleAngle.transform.localPosition = pointOnCircle;

        float cosVal = Mathf.Cos(angle);
        float sinVal = Mathf.Sin(angle);
        float tanVal = Mathf.Abs(cosVal) > 0.01f ? sinVal / cosVal : float.NaN;

        Vector3 worldP = transform.TransformPoint(pointOnCircle);
        Vector3 worldCosEnd = transform.TransformPoint(new Vector3(cosVal * circleRadius, 0, 0));
        Vector3 worldSinEnd = transform.TransformPoint(new Vector3(0, sinVal * circleRadius, 0));

        // 單位圓
        mr.DrawCircle(origin, circleRadius, Vector3.forward, new Color(0.4f, 0.4f, 0.5f), 0.015f);

        // 半徑線（原點到 P）
        mr.DrawLine(origin, worldP, Color.white, 0.02f);

        // cos 線段（紅色水平）
        mr.DrawLine(origin, worldCosEnd, new Color(1f, 0.3f, 0.3f), 0.03f);

        // sin 線段（綠色垂直）
        mr.DrawLine(worldCosEnd, worldP, new Color(0.3f, 0.9f, 0.3f), 0.03f);

        // tan 線段（黃色）
        if (!float.IsNaN(tanVal) && Mathf.Abs(tanVal) < 5f)
        {
            Vector3 tanEnd = transform.TransformPoint(new Vector3(circleRadius, tanVal * circleRadius, 0));
            mr.DrawLine(worldP, tanEnd, new Color(1f, 0.9f, 0.3f), 0.02f);
            mr.DrawDashedLine(origin, tanEnd, new Color(1f, 0.9f, 0.3f, 0.3f));
        }

        // 角度弧
        float angleDeg = angle * Mathf.Rad2Deg;
        if (angleDeg < 0) angleDeg += 360f;
        mr.DrawArc(origin, Vector3.right, angleDeg, 0.5f, new Color(0.8f, 0.8f, 0.3f), 0.015f);

        // 數值
        cosLabel.text = $"cos θ = {cosVal:F3}";
        sinLabel.text = $"sin θ = {sinVal:F3}";
        tanLabel.text = float.IsNaN(tanVal) ? "tan θ = ∞ (未定義)" : $"tan θ = {tanVal:F3}";
        angleLabel.text = $"θ = {angleDeg:F1}°";
    }

    public override bool CheckChallengeComplete()
    {
        float angle = Mathf.Atan2(handleAngle.LocalPosition.y, handleAngle.LocalPosition.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        return Mathf.Abs(angle - 45f) < 3f;
    }

    protected override void OnChallengeCompleted()
    {
        base.OnChallengeCompleted();
        ChallengeSystem.Instance?.MarkCompleted(exhibitName);
    }
}
