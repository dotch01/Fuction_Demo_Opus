using UnityEngine;

// ============================================================
// AxisAngleExhibit.cs — 軸角旋轉
// 拖曳設定旋轉軸 + 角度，3D 物件繞軸旋轉
// ============================================================

public class AxisAngleExhibit : ExhibitBase
{
    private DragHandle handleAxis;
    private DragHandle handleAngle;
    private GameObject targetObj;
    private TextMesh valueLabel;

    public override void BuildExhibit()
    {
        exhibitName = "軸角旋轉 Axis-Angle";
        description = "R(v̂, θ)：繞任意軸旋轉任意角度\n\n比歐拉角更直觀：\n• 用一個方向向量定義旋轉軸\n• 用一個角度定義旋轉量\n• Rodrigues 公式可轉成矩陣\n\n🎮 遊戲應用：\n• 門鉵鏈：繞固定軸旋轉開關\n• 車輪轉動：繞輪軸旋轉\n• Quaternion.AngleAxis() 建構旋轉\n\n拖藍點設軸方向，拖紅點設角度";
        formula = "v' = v·cosθ + (k×v)sinθ + k(k·v)(1-cosθ)";
        challengeDescription = "繞 (1,1,0) 軸旋轉 120°";

        targetObj = CreateStaticPrimitive(PrimitiveType.Cube,
            Vector3.zero, new Vector3(1.5f, 0.6f, 0.6f), new Color(0.9f, 0.5f, 0.3f));

        // 軸向端點
        handleAxis = CreateDragHandle(new Vector3(0.7f, 2f, 0), new Color(0.3f, 0.5f, 1f), 0.15f);

        // 角度滑桿
        handleAngle = CreateDragHandle(new Vector3(0, -2.5f, 0), new Color(1f, 0.3f, 0.3f), 0.12f);
        handleAngle.minBounds = new Vector3(-3f, -2.5f, 0);
        handleAngle.maxBounds = new Vector3(3f, -2.5f, 0);

        CreateLabel(new Vector3(-3f, -2.1f, 0), "-360°", 22, new Color(0.5f, 0.5f, 0.5f));
        CreateLabel(new Vector3(3f, -2.1f, 0), "360°", 22, new Color(0.5f, 0.5f, 0.5f));

        valueLabel = CreateLabel(new Vector3(0, -3.2f, 0), "", 32, Color.white);
    }

    public override void UpdateVisualization()
    {
        if (MathLineRenderer.Instance == null) return;
        var mr = MathLineRenderer.Instance;

        Vector3 axis = handleAxis.LocalPosition.normalized;
        if (axis.sqrMagnitude < 0.001f) axis = Vector3.up;
        float angle = (handleAngle.LocalPosition.x / 3f) * 360f;

        targetObj.transform.localRotation = Quaternion.AngleAxis(angle, axis);

        // 旋轉軸
        Vector3 origin = transform.position;
        Vector3 worldAxis = transform.TransformDirection(axis);
        mr.DrawArrow(origin - worldAxis * 2.5f, origin + worldAxis * 2.5f,
                     new Color(0.3f, 0.5f, 1f, 0.6f), 0.02f);

        // 旋轉弧
        Vector3 perpDir = Vector3.Cross(axis, Vector3.right).normalized;
        if (perpDir.sqrMagnitude < 0.01f) perpDir = Vector3.Cross(axis, Vector3.forward).normalized;
        mr.DrawArc(origin, transform.TransformDirection(perpDir), Mathf.Abs(angle), 1.5f,
                   new Color(1f, 0.7f, 0.3f, 0.5f), 0.02f);

        // 滑桿背景
        mr.DrawLine(transform.TransformPoint(new Vector3(-3, -2.5f, 0)),
                    transform.TransformPoint(new Vector3(3, -2.5f, 0)),
                    new Color(0.3f, 0.3f, 0.3f), 0.008f);

        valueLabel.text = $"軸 = ({axis.x:F2}, {axis.y:F2}, {axis.z:F2})  角度 = {angle:F0}°";
    }

    public override bool CheckChallengeComplete()
    {
        Vector3 axis = handleAxis.LocalPosition.normalized;
        float angle = (handleAngle.LocalPosition.x / 3f) * 360f;
        // 檢查軸接近 (1,1,0).normalized 且角度接近 120°
        float axisDot = Vector3.Dot(axis, new Vector3(1, 1, 0).normalized);
        return Mathf.Abs(axisDot) > 0.9f && Mathf.Abs(Mathf.Abs(angle) - 120f) < 15f;
    }

    protected override void OnChallengeCompleted()
    {
        base.OnChallengeCompleted();
        ChallengeSystem.Instance?.MarkCompleted(exhibitName);
    }
}
