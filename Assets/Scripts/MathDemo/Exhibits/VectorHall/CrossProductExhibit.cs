using UnityEngine;

// ============================================================
// CrossProductExhibit.cs — 叉積互動展品
// 拖曳兩向量，顯示叉積方向 + 平行四邊形面積
// 挑戰：讓面積 > 4
// ============================================================

public class CrossProductExhibit : ExhibitBase
{
    private DragHandle handleA;
    private DragHandle handleB;
    private TextMesh crossLabel;
    private TextMesh areaLabel;

    public override void BuildExhibit()
    {
        exhibitName = "叉積 Cross Product";
        description = "a × b = |a||b|sinθ · n̂\n\n叉積產生垂直於兩向量的第三向量\n大小 = 平行四邊形面積\n方向由右手定則決定\n\n🎮 遊戲應用：\n• 計算多邊形面法線（光照必備）\n• 判斷目標在角色左邊還是右邊\n• 計算扭力（物理引擎旋轉力）\n\n拖曳紅藍球體改變向量";
        formula = "a × b = (ay*bz - az*by, az*bx - ax*bz, ax*by - ay*bx)";
        challengeDescription = "讓平行四邊形面積 > 4";

        CreateStaticPrimitive(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.12f, Color.white);

        handleA = CreateDragHandle(new Vector3(2f, 0f, 0), new Color(1f, 0.3f, 0.3f), 0.18f, DragHandle.DragPlane.XY);
        handleB = CreateDragHandle(new Vector3(0f, 2f, 0), new Color(0.3f, 0.5f, 1f), 0.18f, DragHandle.DragPlane.XY);

        crossLabel = CreateLabel(new Vector3(0, -2f, 0), "", 38, Color.white);
        areaLabel = CreateLabel(new Vector3(0, -2.5f, 0), "", 35, new Color(0.5f, 1f, 0.7f));
    }

    public override void UpdateVisualization()
    {
        if (MathLineRenderer.Instance == null) return;
        var mr = MathLineRenderer.Instance;

        Vector3 origin = transform.position;
        Vector3 a = handleA.LocalPosition;
        Vector3 b = handleB.LocalPosition;

        Vector3 worldA = transform.TransformPoint(a);
        Vector3 worldB = transform.TransformPoint(b);

        // 向量箭頭
        mr.DrawArrow(origin, worldA, new Color(1f, 0.3f, 0.3f), 0.035f);
        mr.DrawArrow(origin, worldB, new Color(0.3f, 0.5f, 1f), 0.035f);

        // 叉積（2D 情況下 z 分量）
        Vector3 cross = Vector3.Cross(a, b);
        float area = cross.magnitude;

        // 平行四邊形
        if (area > 0.01f)
        {
            Vector3 worldAB = transform.TransformPoint(a + b);
            mr.DrawLine(worldA, worldAB, new Color(1f, 0.8f, 0.3f, 0.4f), 0.01f);
            mr.DrawLine(worldB, worldAB, new Color(1f, 0.8f, 0.3f, 0.4f), 0.01f);

            // 叉積方向箭頭（z 方向，縮放顯示）
            float displayScale = Mathf.Min(cross.z * 0.3f, 2f);
            Vector3 crossEnd = origin + Vector3.forward * displayScale;
            mr.DrawArrow(origin, crossEnd, new Color(0.3f, 1f, 0.5f), 0.04f);
        }

        crossLabel.text = $"a × b = (0, 0, {cross.z:F2})";
        areaLabel.text = $"面積 Area = {area:F2}";

        if (area > 4f)
            areaLabel.color = new Color(0.3f, 1f, 0.5f);
        else
            areaLabel.color = new Color(0.5f, 1f, 0.7f);
    }

    public override bool CheckChallengeComplete()
    {
        Vector3 cross = Vector3.Cross(handleA.LocalPosition, handleB.LocalPosition);
        return cross.magnitude > 4f;
    }

    protected override void OnChallengeCompleted()
    {
        base.OnChallengeCompleted();
        ChallengeSystem.Instance?.MarkCompleted(exhibitName);
    }
}
