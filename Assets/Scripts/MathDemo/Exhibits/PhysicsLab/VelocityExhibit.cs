using UnityEngine;

// ============================================================
// VelocityExhibit.cs — 速度向量與拋射運動
// 拖曳初速方向和大小，球以拋物線飛行
// ============================================================

public class VelocityExhibit : ExhibitBase
{
    private DragHandle handleVelocity;
    private GameObject projectile;
    private TextMesh infoLabel;
    private TextMesh heightLabel;

    private float simTime;
    private bool simRunning;
    private Vector3 initialVelocity;
    private Vector3 startPos = new Vector3(-3, -1.5f, 0);
    private float gravity = 9.8f;

    public override void BuildExhibit()
    {
        exhibitName = "速度向量 Velocity";
        description = "v(t) = v₀ + g·t\nposition(t) = p₀ + v₀t + ½gt²\n\n🎮 遊戲應用：\n• 手榴彈/弓箭的拋物線軌跡預覽\n• 憤怒鳥式投射物理\n• 跳躍軌跡預測（能否跳到平台）\n• 子彈重力下墜補償\n\n拖曳箭頭設定初始速度\n按 E 發射觀察拋物線軌跡";
        formula = "p(t) = p₀ + v₀t + ½gt²";
        challengeDescription = "讓球落到 X > 3 的位置";

        // 地面
        CreateStaticPrimitive(PrimitiveType.Cube,
            new Vector3(0, -1.8f, 0), new Vector3(8f, 0.05f, 0.5f), new Color(0.4f, 0.4f, 0.4f));

        // 拋射物
        projectile = CreateStaticPrimitive(PrimitiveType.Sphere,
            startPos, Vector3.one * 0.3f, new Color(1f, 0.5f, 0.2f));

        // 速度控制
        handleVelocity = CreateDragHandle(new Vector3(-1f, 1f, 0), new Color(0.3f, 0.8f, 1f), 0.15f);

        infoLabel = CreateLabel(new Vector3(0, -2.5f, 0), "", 30, Color.white);
        heightLabel = CreateLabel(new Vector3(0, -3.2f, 0), "", 28, new Color(0.7f, 0.8f, 1f));
    }

    public override void UpdateVisualization()
    {
        if (MathLineRenderer.Instance == null) return;
        var mr = MathLineRenderer.Instance;

        initialVelocity = (handleVelocity.LocalPosition - startPos) * 2f;

        if (simRunning)
        {
            simTime += Time.deltaTime;
            Vector3 pos = CalculatePosition(simTime);
            projectile.transform.localPosition = pos;

            // 即時速度
            Vector3 vel = initialVelocity + Vector3.down * gravity * simTime;
            mr.DrawArrow(transform.TransformPoint(pos),
                         transform.TransformPoint(pos + vel.normalized * 0.8f),
                         Color.white, 0.02f);

            if (pos.y < -1.8f)
            {
                simRunning = false;
                infoLabel.text = $"落地！水平距離 = {pos.x - startPos.x:F2}  飛行時間 = {simTime:F2}s";
            }
        }
        else
        {
            projectile.transform.localPosition = startPos;
        }

        // 速度向量預覽
        mr.DrawArrow(transform.TransformPoint(startPos),
                     transform.TransformPoint(startPos + initialVelocity * 0.3f),
                     new Color(0.3f, 0.8f, 1f), 0.03f);

        // 預測拋物線
        int steps = 40;
        for (int i = 0; i < steps; i++)
        {
            float t0 = (float)i / steps * 3f;
            float t1 = (float)(i + 1) / steps * 3f;
            Vector3 p0 = CalculatePosition(t0);
            Vector3 p1 = CalculatePosition(t1);

            if (p0.y < -1.8f) break;

            mr.DrawLine(transform.TransformPoint(p0), transform.TransformPoint(p1),
                        new Color(1f, 0.9f, 0.3f, 0.4f), 0.01f);
        }

        // 最大高度
        float maxH = CalculateMaxHeight();
        heightLabel.text = $"|v₀| = {initialVelocity.magnitude:F1}    最大高度 h = {maxH:F2}    h = vy²/(2g)";

        if (!simRunning)
            infoLabel.text = "按 E 發射！";
    }

    private Vector3 CalculatePosition(float t)
    {
        return startPos + initialVelocity * t + 0.5f * Vector3.down * gravity * t * t;
    }

    private float CalculateMaxHeight()
    {
        float vy = initialVelocity.y;
        if (vy <= 0) return 0;
        return vy * vy / (2f * gravity);
    }

    protected override void OnChallengeStart()
    {
        simTime = 0;
        simRunning = true;
    }

    public override bool CheckChallengeComplete()
    {
        if (simRunning) return false;
        return projectile.transform.localPosition.x > 3f;
    }

    protected override void OnChallengeCompleted()
    {
        base.OnChallengeCompleted();
        ChallengeSystem.Instance?.MarkCompleted(exhibitName);
    }
}
