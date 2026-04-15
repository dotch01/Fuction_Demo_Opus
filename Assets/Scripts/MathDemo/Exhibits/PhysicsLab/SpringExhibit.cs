using UnityEngine;

// ============================================================
// SpringExhibit.cs — 彈簧 (胡克定律 + 阻尼振盪)
// 拖拉彈簧端點然後放開觀察振盪
// ============================================================

public class SpringExhibit : ExhibitBase
{
    private DragHandle handleEnd;
    private GameObject mass;
    private TextMesh forceLabel;
    private TextMesh physicsLabel;

    private Vector3 anchorPos = new Vector3(0, 2f, 0);
    private Vector3 restPos = new Vector3(0, 0, 0);
    private float springK = 15f;
    private float damping = 1.5f;
    private float massValue = 1f;

    private Vector3 velocity;
    private bool simulating = false;

    public override void BuildExhibit()
    {
        exhibitName = "彈簧 Spring (Hooke's Law)";
        description = "F = -kx - bv\n\n• k = 彈性係數（越大越硬）\n• x = 偏移量（離平衡點的距離）\n• b = 阻尼係數（摩擦力）\n• v = 速度\n\n🎮 遊戲應用：\n• 車輛懸吊系統模擬\n• 布料/頭髮物理模擬\n• 相機震動後的回彈\n• 果凍/軟體物件效果\n\n向下拖曳紅球再放開，觀察阻尼振盪";
        formula = "F = -kx - bv    a = F/m";
        challengeDescription = "拉到 y < -2 再放開，觀察振盪";

        // 錨點
        CreateStaticPrimitive(PrimitiveType.Cube,
            anchorPos, new Vector3(1f, 0.15f, 0.15f), new Color(0.5f, 0.5f, 0.5f));

        // 質量塊
        mass = CreateStaticPrimitive(PrimitiveType.Cube,
            restPos, new Vector3(0.6f, 0.6f, 0.3f), new Color(1f, 0.4f, 0.3f));

        handleEnd = CreateDragHandle(new Vector3(0, 0, 0), new Color(1f, 0.4f, 0.3f), 0.18f);
        handleEnd.minBounds = new Vector3(-0.5f, -4f, 0);
        handleEnd.maxBounds = new Vector3(0.5f, 4f, 0);

        forceLabel = CreateLabel(new Vector3(2.5f, 0, 0), "", 28, new Color(1f, 0.6f, 0.3f));
        physicsLabel = CreateLabel(new Vector3(0, -3.5f, 0), "", 26, Color.white);
    }

    public override void UpdateVisualization()
    {
        if (MathLineRenderer.Instance == null) return;
        var mr = MathLineRenderer.Instance;

        Vector3 currentPos;

        if (simulating)
        {
            // 物理模擬
            currentPos = mass.transform.localPosition;
            Vector3 displacement = currentPos - restPos;
            Vector3 springForce = -springK * displacement;
            Vector3 dampingForce = -damping * velocity;
            Vector3 totalForce = springForce + dampingForce;

            Vector3 acceleration = totalForce / massValue;
            velocity += acceleration * Time.deltaTime;
            currentPos += velocity * Time.deltaTime;

            mass.transform.localPosition = currentPos;
            handleEnd.transform.localPosition = currentPos;

            // 停止條件
            if (velocity.magnitude < 0.01f && displacement.magnitude < 0.02f)
            {
                simulating = false;
                velocity = Vector3.zero;
            }
        }
        else
        {
            currentPos = handleEnd.LocalPosition;
            mass.transform.localPosition = currentPos;
        }

        // 繪製彈簧（之字形）
        DrawSpring(mr, transform.TransformPoint(anchorPos), transform.TransformPoint(currentPos), 12);

        // 力的方向箭頭
        Vector3 disp = currentPos - restPos;
        Vector3 force = -springK * disp;
        if (force.magnitude > 0.1f)
        {
            mr.DrawArrow(transform.TransformPoint(currentPos),
                         transform.TransformPoint(currentPos + force.normalized * Mathf.Min(force.magnitude * 0.1f, 1.5f)),
                         new Color(0.3f, 1f, 0.5f), 0.03f);
        }

        // 平衡點虛線
        mr.DrawDashedLine(transform.TransformPoint(restPos + Vector3.left * 1.5f),
                          transform.TransformPoint(restPos + Vector3.right * 1.5f),
                          new Color(0.5f, 0.5f, 0.5f, 0.4f));

        forceLabel.text = $"F = {force.y:F1}N\nx = {disp.y:F2}";
        physicsLabel.text = $"k = {springK}    b = {damping}    m = {massValue}    v = {velocity.y:F2}";
    }

    private void DrawSpring(MathLineRenderer mr, Vector3 top, Vector3 bottom, int coils)
    {
        Vector3 dir = (bottom - top);
        float length = dir.magnitude;
        dir /= length;

        Vector3 right = Vector3.Cross(dir, Vector3.forward).normalized;
        if (right.sqrMagnitude < 0.01f) right = Vector3.Cross(dir, Vector3.up).normalized;

        float coilWidth = 0.3f;
        int pointsPerCoil = 4;
        int total = coils * pointsPerCoil;

        Vector3 prev = top;
        for (int i = 1; i <= total; i++)
        {
            float t = (float)i / total;
            Vector3 center = Vector3.Lerp(top, bottom, t);

            float side = ((i % pointsPerCoil) < pointsPerCoil / 2) ? 1f : -1f;
            float offset = (i % pointsPerCoil == 0 || i % pointsPerCoil == pointsPerCoil / 2) ? 0 : coilWidth;

            Vector3 point = center + right * offset * side;
            mr.DrawLine(prev, point, new Color(0.7f, 0.7f, 0.8f), 0.015f);
            prev = point;
        }
    }

    void OnMouseUp()
    {
        // 開始模擬（放手時）
        if (handleEnd.LocalPosition != restPos)
        {
            simulating = true;
            velocity = Vector3.zero;
        }
    }

    protected override void OnChallengeStart()
    {
        // 先讓玩家拉到 y < -2
    }

    public override bool CheckChallengeComplete()
    {
        return simulating && Mathf.Abs(handleEnd.LocalPosition.y) < 0.1f && velocity.magnitude > 0.1f;
    }

    protected override void OnChallengeCompleted()
    {
        base.OnChallengeCompleted();
        ChallengeSystem.Instance?.MarkCompleted(exhibitName);
    }
}
