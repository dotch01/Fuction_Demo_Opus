using UnityEngine;

// ============================================================
// MaxBounceHeightExhibit.cs — 最大彈跳高度
// h = v²/(2g) 物理公式
// ============================================================

public class MaxBounceHeightExhibit : ExhibitBase
{
    private DragHandle handleV, handleG;
    private TextMesh heightLabel, formulaLabel, infoLabel;
    private GameObject ball;
    private float simTime;
    private float v0, gravity;

    public override void BuildExhibit()
    {
        exhibitName = "最大跳躍高度";
        description = "角色跳躍的最大高度：\n\nh = v₀² / (2g)\n\n• v₀ = 初速度（向上）\n• g = 重力加速度\n反推：要跳 h 高 → v₀ = √(2gh)\n\n🎮 遊戲應用：\n• 關卡設計：平台高度差 ≤ h 才能跳到\n• 瑪利歐式跳躍手感調校\n• 不同重力星球的跳躍高度差異\n• 雙跳/噴射背包的額外高度計算\n\n拖曳控制初速和重力";
        formula = "h = v₀²/(2g)    v₀ = √(2gh)";
        challengeDescription = "讓球跳超過 2 單位高度";

        handleV = CreateDragHandle(new Vector3(-2.5f, 0, 0), new Color(0.3f, 0.8f, 1f), 0.12f);
        handleV.minBounds = new Vector3(-2.5f, -1.5f, 0);
        handleV.maxBounds = new Vector3(-2.5f, 1.5f, 0);

        handleG = CreateDragHandle(new Vector3(2.5f, 0, 0), new Color(1f, 0.5f, 0.3f), 0.12f);
        handleG.minBounds = new Vector3(2.5f, -1.5f, 0);
        handleG.maxBounds = new Vector3(2.5f, 1.5f, 0);

        ball = CreateStaticPrimitive(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.2f, new Color(0.3f, 0.9f, 0.5f));

        CreateLabel(new Vector3(-2.5f, -2f, 0), "初速 v₀", 20, new Color(0.5f, 0.5f, 0.6f));
        CreateLabel(new Vector3(2.5f, -2f, 0), "重力 g", 20, new Color(0.5f, 0.5f, 0.6f));

        heightLabel = CreateLabel(new Vector3(0, -2.5f, 0), "", 30, Color.white);
        formulaLabel = CreateLabel(new Vector3(0, -3.1f, 0), "", 26, new Color(0.7f, 0.8f, 1f));
        infoLabel = CreateLabel(new Vector3(0, 2.5f, 0), "", 22, new Color(0.6f, 0.6f, 0.7f));
    }

    public override void UpdateVisualization()
    {
        v0 = Mathf.Lerp(1f, 8f, Mathf.InverseLerp(-1.5f, 1.5f, handleV.LocalPosition.y));
        gravity = Mathf.Lerp(2f, 15f, Mathf.InverseLerp(-1.5f, 1.5f, handleG.LocalPosition.y));

        float maxH = v0 * v0 / (2f * gravity);
        float period = 2f * v0 / gravity;

        simTime += Time.deltaTime;
        float t = simTime % period;
        float y = v0 * t - 0.5f * gravity * t * t;
        y = Mathf.Max(0, y);

        ball.transform.localPosition = new Vector3(0, y * 0.4f - 1f, 0);

        // 畫最大高度線
        var mr = MathLineRenderer.Instance;
        if (mr != null)
        {
            float maxHScreen = maxH * 0.4f - 1f;
            mr.DrawLine(transform.TransformPoint(new Vector3(-1.5f, maxHScreen, 0)),
                        transform.TransformPoint(new Vector3(1.5f, maxHScreen, 0)),
                        new Color(1f, 0.85f, 0.2f, 0.5f), 0.005f);
            infoLabel.text = $"▼ 最高點 h = {maxH:F2}";
            infoLabel.transform.localPosition = new Vector3(0, maxHScreen + 0.3f, 0);
        }

        heightLabel.text = $"h = {v0:F1}² / (2×{gravity:F1}) = {maxH:F2}";
        formulaLabel.text = $"需要 v₀ = √(2×{gravity:F1}×{maxH:F1}) = {v0:F2}";
    }

    public override bool CheckChallengeComplete()
    {
        return v0 * v0 / (2f * gravity) > 2f;
    }
}
