using UnityEngine;

// ============================================================
// QuaternionExhibit.cs — 四元數旋轉
// 拖曳旋轉軸方向 + 角度滑桿，3D 物件即時旋轉
// ============================================================

public class QuaternionExhibit : ExhibitBase
{
    private DragHandle handleAxis;
    private DragHandle handleAngle; // x 位置當角度
    private GameObject targetCube;
    private TextMesh quatLabel;
    private TextMesh axisLabel;

    public override void BuildExhibit()
    {
        exhibitName = "四元數 Quaternion";
        description = "q = cos(θ/2) + sin(θ/2)(xi + yj + zk)\n\n四元數是 3D 旋轉的最佳表示法：\n• 沒有萬向節鎖\n• 可平滑插值（Slerp）\n• 4 個分量比矩陣更節省\n\n🎮 遊戲應用：\n• Unity transform.rotation 底層就是 Quaternion\n• 骨骼動畫的每個關節旋轉\n• 第三人稱相機的軋道旋轉\n\n拖藍點設旋轉軸，拖紅點設角度";
        formula = "q = (w, x, y, z) = (cos θ/2, sin θ/2 · axis)";
        challengeDescription = "旋轉物件 180° 翻轉";

        targetCube = CreateStaticPrimitive(PrimitiveType.Cube,
            Vector3.zero, Vector3.one * 1.2f, new Color(0.4f, 0.6f, 0.9f));
        // 加回 collider 不需要，保持無 collider

        // 旋轉軸控制（在球面上）
        handleAxis = CreateDragHandle(new Vector3(0, 2.5f, 0), new Color(0.3f, 0.5f, 1f), 0.15f);

        // 角度控制（x 軸 = 角度 -180~180）
        handleAngle = CreateDragHandle(new Vector3(1f, -2f, 0), new Color(1f, 0.3f, 0.3f), 0.15f);
        handleAngle.minBounds = new Vector3(-3f, -2f, 0);
        handleAngle.maxBounds = new Vector3(3f, -2f, 0);

        quatLabel = CreateLabel(new Vector3(0, -2.8f, 0), "", 32, Color.white);
        axisLabel = CreateLabel(new Vector3(0, -3.3f, 0), "", 30, new Color(0.7f, 0.8f, 1f));

        // 角度刻度線
        CreateLabel(new Vector3(-3f, -1.6f, 0), "-180°", 25, new Color(0.5f, 0.5f, 0.5f));
        CreateLabel(new Vector3(0, -1.6f, 0), "0°", 25, new Color(0.5f, 0.5f, 0.5f));
        CreateLabel(new Vector3(3f, -1.6f, 0), "180°", 25, new Color(0.5f, 0.5f, 0.5f));
    }

    public override void UpdateVisualization()
    {
        if (MathLineRenderer.Instance == null) return;
        var mr = MathLineRenderer.Instance;

        // 旋轉軸
        Vector3 axisDir = handleAxis.LocalPosition.normalized;
        if (axisDir.sqrMagnitude < 0.001f) axisDir = Vector3.up;

        // 角度（x 位置映射到 -180~180）
        float angle = (handleAngle.LocalPosition.x / 3f) * 180f;

        // 四元數
        Quaternion q = Quaternion.AngleAxis(angle, axisDir);
        targetCube.transform.localRotation = q;

        // 繪製旋轉軸
        Vector3 origin = transform.position;
        mr.DrawArrow(origin - transform.TransformDirection(axisDir) * 2f,
                     origin + transform.TransformDirection(axisDir) * 2f,
                     new Color(0.3f, 0.5f, 1f, 0.6f), 0.02f);

        // 角度條背景
        mr.DrawLine(transform.TransformPoint(new Vector3(-3, -2, 0)),
                    transform.TransformPoint(new Vector3(3, -2, 0)),
                    new Color(0.3f, 0.3f, 0.3f), 0.01f);

        // 四元數值
        quatLabel.text = $"q = ({q.w:F2}, {q.x:F2}, {q.y:F2}, {q.z:F2})";
        axisLabel.text = $"軸 = ({axisDir.x:F2}, {axisDir.y:F2}, {axisDir.z:F2})    角度 = {angle:F0}°";
    }

    public override bool CheckChallengeComplete()
    {
        float angle = (handleAngle.LocalPosition.x / 3f) * 180f;
        return Mathf.Abs(Mathf.Abs(angle) - 180f) < 10f;
    }

    protected override void OnChallengeCompleted()
    {
        base.OnChallengeCompleted();
        ChallengeSystem.Instance?.MarkCompleted(exhibitName);
    }
}
