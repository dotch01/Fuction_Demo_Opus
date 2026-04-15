using UnityEngine;

// ============================================================
// SlerpExhibit.cs — 球面線性插值
// 拖曳 t 滑桿，觀察兩個旋轉間的平滑插值
// ============================================================

public class SlerpExhibit : ExhibitBase
{
    private DragHandle handleT;
    private GameObject objA, objB, objInterp;
    private TextMesh valueLabel;

    private Quaternion rotA = Quaternion.Euler(0, 0, -45);
    private Quaternion rotB = Quaternion.Euler(0, 0, 135);

    public override void BuildExhibit()
    {
        exhibitName = "Slerp 球面插值";
        description = "slerp(q1, q2, t)：在兩個旋轉之間平滑插值\n\n• Lerp 在直線上插值 → 速度不均勻\n• Slerp 在球面上插值 → 等角速度旋轉\n\n🎮 遊戲應用：\n• 鏡頭過場：從 A 角度平滑轉到 B\n• 角色轉身動畫的旋轉混合\n• 飛彈追蹤：逐漸旋轉朝向目標\n• 死亡回放的慢動作鏡頭\n\n拖曳 t 滑桿觀察中間旋轉";
        formula = "slerp(q₁,q₂,t) = q₁(q₁⁻¹q₂)ᵗ";
        challengeDescription = "讓 t 精準停在 0.50（±0.03）";

        // 起始旋轉物件
        objA = CreateStaticPrimitive(PrimitiveType.Cube,
            new Vector3(-3f, 0.5f, 0), new Vector3(1.5f, 0.3f, 0.3f), new Color(1f, 0.3f, 0.3f));
        objA.transform.localRotation = rotA;
        CreateLabel(new Vector3(-3f, -0.8f, 0), "q₁ (t=0)", 25, new Color(1f, 0.4f, 0.4f));

        // 結束旋轉物件
        objB = CreateStaticPrimitive(PrimitiveType.Cube,
            new Vector3(3f, 0.5f, 0), new Vector3(1.5f, 0.3f, 0.3f), new Color(0.3f, 0.5f, 1f));
        objB.transform.localRotation = rotB;
        CreateLabel(new Vector3(3f, -0.8f, 0), "q₂ (t=1)", 25, new Color(0.4f, 0.5f, 1f));

        // 插值物件
        objInterp = CreateStaticPrimitive(PrimitiveType.Cube,
            new Vector3(0, 0.5f, 0), new Vector3(1.5f, 0.3f, 0.3f), new Color(1f, 0.9f, 0.3f));

        // t 滑桿
        handleT = CreateDragHandle(new Vector3(0, -2f, 0), new Color(1f, 0.9f, 0.3f), 0.15f);
        handleT.minBounds = new Vector3(-3f, -2f, 0);
        handleT.maxBounds = new Vector3(3f, -2f, 0);

        CreateLabel(new Vector3(-3f, -1.6f, 0), "t=0", 22, new Color(0.5f, 0.5f, 0.5f));
        CreateLabel(new Vector3(3f, -1.6f, 0), "t=1", 22, new Color(0.5f, 0.5f, 0.5f));

        valueLabel = CreateLabel(new Vector3(0, -2.8f, 0), "", 34, Color.white);
    }

    public override void UpdateVisualization()
    {
        if (MathLineRenderer.Instance == null) return;
        var mr = MathLineRenderer.Instance;

        float t = Mathf.InverseLerp(-3f, 3f, handleT.LocalPosition.x);

        // Slerp 插值
        Quaternion interp = Quaternion.Slerp(rotA, rotB, t);
        objInterp.transform.localRotation = interp;

        // 插值物件位置也跟著 t 移動
        objInterp.transform.localPosition = new Vector3(Mathf.Lerp(-3f, 3f, t), 0.5f, 0);

        // 繪製旋轉弧
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        for (int i = 0; i <= 20; i++)
        {
            float t0 = (float)i / 20;
            float t1 = (float)(i + 1) / 20;
            Quaternion q0 = Quaternion.Slerp(rotA, rotB, t0);
            Quaternion q1 = Quaternion.Slerp(rotA, rotB, t1);
            Vector3 dir0 = q0 * Vector3.right * 1.5f;
            Vector3 dir1 = q1 * Vector3.right * 1.5f;

            Color arcColor = t0 <= t ? new Color(1f, 0.8f, 0.2f, 0.5f) : new Color(0.4f, 0.4f, 0.4f, 0.3f);
            mr.DrawLine(origin + dir0, origin + dir1, arcColor, 0.015f);
        }

        // 滑桿背景
        mr.DrawLine(transform.TransformPoint(new Vector3(-3, -2, 0)),
                    transform.TransformPoint(new Vector3(3, -2, 0)),
                    new Color(0.3f, 0.3f, 0.3f), 0.008f);

        valueLabel.text = $"t = {t:F2}    角度 = {Quaternion.Angle(rotA, interp):F1}° / {Quaternion.Angle(rotA, rotB):F1}°";
    }

    public override bool CheckChallengeComplete()
    {
        float t = Mathf.InverseLerp(-3f, 3f, handleT.LocalPosition.x);
        return Mathf.Abs(t - 0.5f) < 0.03f;
    }

    protected override void OnChallengeCompleted()
    {
        base.OnChallengeCompleted();
        ChallengeSystem.Instance?.MarkCompleted(exhibitName);
    }
}
