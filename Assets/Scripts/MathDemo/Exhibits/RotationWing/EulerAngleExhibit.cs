using UnityEngine;

// ============================================================
// EulerAngleExhibit.cs — 歐拉角 + 萬向節鎖示範
// 三個滑桿控制 Pitch/Yaw/Roll，展示 Gimbal Lock
// ============================================================

public class EulerAngleExhibit : ExhibitBase
{
    private DragHandle handlePitch;
    private DragHandle handleYaw;
    private DragHandle handleRoll;
    private GameObject targetObj;
    private TextMesh eulerLabel;
    private TextMesh gimbalWarning;

    // 三個旋轉環（Gimbal）
    private GameObject ringX, ringY, ringZ;

    public override void BuildExhibit()
    {
        exhibitName = "歐拉角 Euler Angles";
        description = "Pitch (X) / Yaw (Y) / Roll (Z)\n\n歐拉角直觀但有致命缺陷：\n• Gimbal Lock 萬向節鎖\n  → 當 Pitch = ±90° 時 Yaw 和 Roll 重合\n  → 失去一個自由度\n\n🎮 遊戲應用：\n• Inspector 面板的旋轉欄位就是歐拉角\n• 飛行模擬的俰仰/偏航/翻滾\n• FPS 相機：Pitch 限制在 ±89° 避鎖死\n\n拖滑桿到 Pitch ≈ 90° 觀察鎖死現象";
        formula = "R = Rz(roll) × Rx(pitch) × Ry(yaw)";
        challengeDescription = "觸發萬向節鎖（Pitch 接近 90°）";

        // 目標物件
        targetObj = CreateStaticPrimitive(PrimitiveType.Cube,
            Vector3.zero, new Vector3(1.2f, 0.8f, 1.8f), new Color(0.5f, 0.7f, 0.9f));

        // 三個滑桿
        float sliderY = -2.2f;
        CreateLabel(new Vector3(-3.5f, sliderY + 0.3f, 0), "Pitch X", 25, new Color(1f, 0.4f, 0.4f));
        handlePitch = CreateDragHandle(new Vector3(0, sliderY, 0), new Color(1f, 0.4f, 0.4f), 0.12f);
        handlePitch.minBounds = new Vector3(-3f, sliderY, 0);
        handlePitch.maxBounds = new Vector3(3f, sliderY, 0);

        sliderY = -2.8f;
        CreateLabel(new Vector3(-3.5f, sliderY + 0.3f, 0), "Yaw Y", 25, new Color(0.4f, 1f, 0.4f));
        handleYaw = CreateDragHandle(new Vector3(0, sliderY, 0), new Color(0.4f, 1f, 0.4f), 0.12f);
        handleYaw.minBounds = new Vector3(-3f, sliderY, 0);
        handleYaw.maxBounds = new Vector3(3f, sliderY, 0);

        sliderY = -3.4f;
        CreateLabel(new Vector3(-3.5f, sliderY + 0.3f, 0), "Roll Z", 25, new Color(0.4f, 0.4f, 1f));
        handleRoll = CreateDragHandle(new Vector3(0, sliderY, 0), new Color(0.4f, 0.4f, 1f), 0.12f);
        handleRoll.minBounds = new Vector3(-3f, sliderY, 0);
        handleRoll.maxBounds = new Vector3(3f, sliderY, 0);

        eulerLabel = CreateLabel(new Vector3(0, -4.2f, 0), "", 30, Color.white);
        gimbalWarning = CreateLabel(new Vector3(0, 2.5f, 0), "", 40, new Color(1f, 0.3f, 0.2f));
    }

    public override void UpdateVisualization()
    {
        if (MathLineRenderer.Instance == null) return;
        var mr = MathLineRenderer.Instance;

        float pitch = (handlePitch.LocalPosition.x / 3f) * 90f;
        float yaw = (handleYaw.LocalPosition.x / 3f) * 180f;
        float roll = (handleRoll.LocalPosition.x / 3f) * 180f;

        targetObj.transform.localRotation = Quaternion.Euler(pitch, yaw, roll);

        // 繪製三軸
        Vector3 origin = transform.position;
        Vector3 fwd = targetObj.transform.forward;
        Vector3 up = targetObj.transform.up;
        Vector3 right = targetObj.transform.right;

        mr.DrawArrow(origin, origin + fwd * 1.8f, new Color(0.3f, 0.3f, 1f), 0.025f);
        mr.DrawArrow(origin, origin + up * 1.8f, new Color(0.3f, 1f, 0.3f), 0.025f);
        mr.DrawArrow(origin, origin + right * 1.8f, new Color(1f, 0.3f, 0.3f), 0.025f);

        // 滑桿背景線
        for (int i = 0; i < 3; i++)
        {
            float y = -2.2f - i * 0.6f;
            mr.DrawLine(transform.TransformPoint(new Vector3(-3, y, 0)),
                       transform.TransformPoint(new Vector3(3, y, 0)),
                       new Color(0.3f, 0.3f, 0.3f), 0.008f);
        }

        eulerLabel.text = $"Euler = ({pitch:F0}°, {yaw:F0}°, {roll:F0}°)";

        // 萬向節鎖偵測
        bool gimbalLock = Mathf.Abs(Mathf.Abs(pitch) - 90f) < 8f;
        if (gimbalLock)
            gimbalWarning.text = "⚠ GIMBAL LOCK ⚠\n萬向節鎖！";
        else
            gimbalWarning.text = "";
    }

    public override bool CheckChallengeComplete()
    {
        float pitch = (handlePitch.LocalPosition.x / 3f) * 90f;
        return Mathf.Abs(Mathf.Abs(pitch) - 90f) < 5f;
    }

    protected override void OnChallengeCompleted()
    {
        base.OnChallengeCompleted();
        ChallengeSystem.Instance?.MarkCompleted(exhibitName);
    }
}
