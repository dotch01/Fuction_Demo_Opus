using UnityEngine;

// ============================================================
// IntegralDistanceExhibit.cs — 積分累距離
// 每幀 ‖v‖Δt 對比解析解
// ============================================================

public class IntegralDistanceExhibit : ExhibitBase
{
    private TextMesh numericLabel, analyticLabel, errorLabel, infoLabel;
    private GameObject ball;
    private float simTime;
    private float numericDist, analyticDist;

    public override void BuildExhibit()
    {
        exhibitName = "積分累距離";
        description = "物體沿曲線運動的距離：\n\n解析解：S = ∫‖v(t)‖dt\n\n數值解（遊戲做法）：\n  distance += ‖velocity‖ × deltaTime\n\n🎮 遊戲應用：\n• 賽車里程計/速度錶\n• 角色移動距離統計（成就系統）\n• 物理模擬精確度 vs 效能取捨\n• FixedUpdate vs Update 的積分差異\n\n觀察數值解如何逼近真值";
        formula = "S ≈ Σ ‖v_i‖ · Δt";
        challengeDescription = "讓模擬跑完一圈（誤差 < 5%）";

        ball = CreateStaticPrimitive(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.15f, new Color(0.3f, 0.9f, 0.5f));

        numericLabel = CreateLabel(new Vector3(0, -1.5f, 0), "", 28, new Color(0.3f, 0.9f, 1f));
        analyticLabel = CreateLabel(new Vector3(0, -2.1f, 0), "", 28, new Color(1f, 0.85f, 0.3f));
        errorLabel = CreateLabel(new Vector3(0, -2.7f, 0), "", 26, Color.white);
        infoLabel = CreateLabel(new Vector3(0, 2.2f, 0), "白線 = 軌跡　圓 = 運動中", 22, new Color(0.6f, 0.6f, 0.7f));
    }

    public override void UpdateVisualization()
    {
        var mr = MathLineRenderer.Instance;
        if (mr == null) return;

        simTime += Time.deltaTime;
        float omega = 1.5f;
        float radius = 1.5f;

        // 橢圓軌道
        float x = radius * Mathf.Cos(omega * simTime);
        float y = radius * 0.6f * Mathf.Sin(omega * simTime);
        ball.transform.localPosition = new Vector3(x, y, 0);

        // 速度
        float vx = -radius * omega * Mathf.Sin(omega * simTime);
        float vy = radius * 0.6f * omega * Mathf.Cos(omega * simTime);
        float speed = Mathf.Sqrt(vx * vx + vy * vy);

        // 數值積分
        numericDist += speed * Time.deltaTime;

        // 解析：橢圓周長近似（Ramanujan）
        float a = radius, b = radius * 0.6f;
        float fullCircumference = Mathf.PI * (3 * (a + b) - Mathf.Sqrt((3 * a + b) * (a + 3 * b)));
        float periods = simTime * omega / (2 * Mathf.PI);
        analyticDist = periods * fullCircumference;

        // 畫軌跡
        int steps = 48;
        for (int i = 0; i < steps; i++)
        {
            float t0 = (float)i / steps * Mathf.PI * 2;
            float t1 = (float)(i + 1) / steps * Mathf.PI * 2;
            Vector3 p0 = new Vector3(radius * Mathf.Cos(t0), radius * 0.6f * Mathf.Sin(t0), 0);
            Vector3 p1 = new Vector3(radius * Mathf.Cos(t1), radius * 0.6f * Mathf.Sin(t1), 0);
            mr.DrawLine(transform.TransformPoint(p0), transform.TransformPoint(p1), new Color(0.4f, 0.4f, 0.5f), 0.005f);
        }

        float error = analyticDist > 0.1f ? Mathf.Abs(numericDist - analyticDist) / analyticDist * 100f : 0;
        numericLabel.text = $"數值累加: {numericDist:F2}";
        analyticLabel.text = $"解析解: {analyticDist:F2}";
        errorLabel.text = $"誤差: {error:F2}%    圈數: {periods:F1}";
        errorLabel.color = error < 5f ? new Color(0.3f, 1f, 0.5f) : new Color(1f, 0.5f, 0.3f);
    }

    public override bool CheckChallengeComplete()
    {
        float omega = 1.5f;
        float periods = simTime * omega / (2 * Mathf.PI);
        float a = 1.5f, b = 0.9f;
        float circum = Mathf.PI * (3 * (a + b) - Mathf.Sqrt((3 * a + b) * (a + 3 * b)));
        analyticDist = periods * circum;
        float error = analyticDist > 0.1f ? Mathf.Abs(numericDist - analyticDist) / analyticDist * 100f : 100f;
        return periods >= 1f && error < 5f;
    }
}
