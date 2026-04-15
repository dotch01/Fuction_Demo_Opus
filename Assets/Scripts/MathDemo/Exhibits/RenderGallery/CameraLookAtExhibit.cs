using UnityEngine;

// ============================================================
// CameraLookAtExhibit.cs — LookAt 相機建構
// 逐步建 View 矩陣：Right → Up → Forward
// ============================================================

public class CameraLookAtExhibit : ExhibitBase
{
    private DragHandle handleTarget;
    private TextMesh rightLabel, upLabel, fwdLabel, infoLabel;
    private Vector3 camPos = new Vector3(-1.5f, 0.5f, 0);

    public override void BuildExhibit()
    {
        exhibitName = "LookAt 相機建構";
        description = "View 矩陣 = 讓世界相對於相機\n\n步驟：\n① Forward = normalize(target - eye)\n② Right = normalize(Forward × worldUp)\n③ Up = Right × Forward\n\n🎮 遊戲應用：\n• Transform.LookAt() 的底層數學\n• 第三人稱相機對準角色\n• 砲塔/眼球追蹤目標\n• 過場動畫鏡頭朝向控制\n\n拖曳目標點";
        formula = "V = [R U -F 0; 0 0 0 1] × T(-eye)";
        challengeDescription = "讓 Forward 指向上方（y > 0.8）";

        handleTarget = CreateDragHandle(new Vector3(1.5f, 0.5f, 0), new Color(1f, 0.5f, 0.3f));

        CreateStaticPrimitive(PrimitiveType.Cube, camPos, Vector3.one * 0.2f, new Color(0.3f, 0.5f, 0.8f));

        rightLabel = CreateLabel(new Vector3(0, -1.5f, 0), "", 26, new Color(1f, 0.3f, 0.3f));
        upLabel = CreateLabel(new Vector3(0, -2.1f, 0), "", 26, new Color(0.3f, 1f, 0.3f));
        fwdLabel = CreateLabel(new Vector3(0, -2.7f, 0), "", 26, new Color(0.3f, 0.5f, 1f));
        infoLabel = CreateLabel(new Vector3(0, -3.3f, 0), "", 22, new Color(0.7f, 0.7f, 0.8f));
    }

    public override void UpdateVisualization()
    {
        var mr = MathLineRenderer.Instance;
        if (mr == null) return;

        Vector3 target = handleTarget.LocalPosition;
        Vector3 eye = camPos;

        Vector3 fwd = (target - eye).normalized;
        Vector3 worldUp = Vector3.up;
        Vector3 right = Vector3.Cross(fwd, worldUp).normalized;
        if (right.sqrMagnitude < 0.001f) right = Vector3.right;
        Vector3 up = Vector3.Cross(right, fwd);

        Vector3 ew = transform.TransformPoint(eye);

        // 畫三軸
        float len = 0.8f;
        mr.DrawArrow(ew, ew + transform.TransformDirection(right * len), new Color(1f, 0.3f, 0.3f), 0.012f, 0.06f);
        mr.DrawArrow(ew, ew + transform.TransformDirection(up * len), new Color(0.3f, 1f, 0.3f), 0.012f, 0.06f);
        mr.DrawArrow(ew, ew + transform.TransformDirection(fwd * len), new Color(0.3f, 0.5f, 1f), 0.012f, 0.06f);

        // eye → target 虛線
        mr.DrawLine(ew, transform.TransformPoint(target), new Color(0.5f, 0.5f, 0.6f, 0.4f), 0.005f);

        rightLabel.text = $"Right = ({right.x:F2}, {right.y:F2}, {right.z:F2})";
        upLabel.text = $"Up = ({up.x:F2}, {up.y:F2}, {up.z:F2})";
        fwdLabel.text = $"Forward = ({fwd.x:F2}, {fwd.y:F2}, {fwd.z:F2})";
        infoLabel.text = "View Matrix = Rotation × Translation(-eye)";
    }

    public override bool CheckChallengeComplete()
    {
        Vector3 fwd = (handleTarget.LocalPosition - camPos).normalized;
        return fwd.y > 0.8f;
    }
}
