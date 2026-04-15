using UnityEngine;

// ============================================================
// CoordinateTransformExhibit.cs — 座標系統轉換
// Local/World 空間轉換 + TRS 矩陣
// ============================================================

public class CoordinateTransformExhibit : ExhibitBase
{
    private DragHandle handleObj, handleParent;
    private TextMesh worldLabel, localLabel, matrixLabel;
    private GameObject childCube, parentPivot;

    public override void BuildExhibit()
    {
        exhibitName = "座標轉換 Coordinate Transform";
        description = "物件都有 Local 和 World 座標：\n\nWorld = Parent × Local\nLocal = Parent⁻¹ × World\n\n🎮 遊戲應用：\n• 手持武器：Local 相對於手部骨骼\n• UI 元素：Local 相對於 Canvas\n• transform.InverseTransformPoint()\n• 存檔位置用 Local 避免父物件影響\n\n拖曳黃色物件和藍色父旋轉中心";
        formula = "M_world = T · R · S · v_local";
        challengeDescription = "讓 Local 和 World 座標相差超過 1";

        parentPivot = CreateStaticPrimitive(PrimitiveType.Cube, new Vector3(0, 0.5f, 0), Vector3.one * 0.2f, new Color(0.3f, 0.5f, 0.8f));
        childCube = CreateStaticPrimitive(PrimitiveType.Cube, new Vector3(1f, 0.5f, 0), Vector3.one * 0.25f, new Color(1f, 0.85f, 0.3f));
        childCube.transform.SetParent(parentPivot.transform, true);

        handleObj = CreateDragHandle(new Vector3(1f, 0.5f, 0), new Color(1f, 0.85f, 0.3f), 0.1f);
        handleParent = CreateDragHandle(new Vector3(0, 0.5f, 0), new Color(0.3f, 0.5f, 0.8f), 0.1f);

        worldLabel = CreateLabel(new Vector3(0, -1.5f, 0), "", 28, new Color(0.3f, 0.9f, 1f));
        localLabel = CreateLabel(new Vector3(0, -2.1f, 0), "", 28, new Color(1f, 0.85f, 0.3f));
        matrixLabel = CreateLabel(new Vector3(0, -2.7f, 0), "", 22, new Color(0.6f, 0.6f, 0.7f));
    }

    public override void UpdateVisualization()
    {
        var mr = MathLineRenderer.Instance;
        if (mr == null) return;

        // 旋轉父物件（用時間模擬）
        float angle = Time.time * 30f;
        parentPivot.transform.localPosition = handleParent.LocalPosition;
        parentPivot.transform.localRotation = Quaternion.Euler(0, 0, angle);

        // 子物件跟隨
        childCube.transform.localPosition = handleObj.LocalPosition - handleParent.LocalPosition;

        Vector3 worldPos = childCube.transform.position;
        Vector3 localPos = childCube.transform.localPosition;

        // 畫座標軸
        Vector3 pw = transform.TransformPoint(handleParent.LocalPosition);
        mr.DrawArrow(pw, pw + parentPivot.transform.right * 0.8f, new Color(1f, 0.3f, 0.3f), 0.01f, 0.05f);
        mr.DrawArrow(pw, pw + parentPivot.transform.up * 0.8f, new Color(0.3f, 1f, 0.3f), 0.01f, 0.05f);

        worldLabel.text = $"World: ({worldPos.x:F2}, {worldPos.y:F2}, {worldPos.z:F2})";
        localLabel.text = $"Local: ({localPos.x:F2}, {localPos.y:F2}, {localPos.z:F2})";
        matrixLabel.text = $"Parent 旋轉 {angle:F0}°  →  Local ≠ World";
    }

    public override bool CheckChallengeComplete()
    {
        Vector3 wp = childCube.transform.position;
        Vector3 lp = childCube.transform.localPosition;
        return Vector3.Distance(wp, transform.TransformPoint(lp)) > 1f;
    }
}
