using UnityEngine;

// ============================================================
// ParentChildExhibit.cs — 父子旋轉
// 顯示 M_child = M_parent × M_local
// ============================================================

public class ParentChildExhibit : ExhibitBase
{
    private DragHandle handleParentRot, handleChildRot;
    private TextMesh matLabel, resultLabel;
    private GameObject parentObj, childObj;

    public override void BuildExhibit()
    {
        exhibitName = "父子旋轉 Parent-Child";
        description = "子物件的世界變換 = 父 × 子本地：\n\nM_child = M_parent × M_local\n\n🎮 遊戲應用：\n• 骨骼動畫：肩→上臂→前臂→手的階層\n• 車子：車身→輪子，車身轉輪子跟著轉\n• 裝備系統：武器掛在手骨下\n• 粒子跟隨角色移動\n\n拖曳控制父/子旋轉觀察繼承效果";
        formula = "M_world = M_parent × M_local";
        challengeDescription = "讓父子旋轉合計超過 180°";

        parentObj = CreateStaticPrimitive(PrimitiveType.Cube, new Vector3(0, 0.5f, 0), new Vector3(1.5f, 0.15f, 0.15f), new Color(0.3f, 0.5f, 0.8f));
        childObj = CreateStaticPrimitive(PrimitiveType.Cube, new Vector3(1f, 0.5f, 0), new Vector3(0.8f, 0.12f, 0.12f), new Color(1f, 0.5f, 0.3f));
        childObj.transform.SetParent(parentObj.transform, true);
        childObj.transform.localPosition = new Vector3(1.2f, 0, 0);

        handleParentRot = CreateDragHandle(new Vector3(-2f, -1.5f, 0), new Color(0.3f, 0.5f, 0.85f), 0.12f);
        handleParentRot.minBounds = new Vector3(-2f, -2.5f, 0);
        handleParentRot.maxBounds = new Vector3(-2f, -0.5f, 0);

        handleChildRot = CreateDragHandle(new Vector3(2f, -1.5f, 0), new Color(1f, 0.5f, 0.3f), 0.12f);
        handleChildRot.minBounds = new Vector3(2f, -2.5f, 0);
        handleChildRot.maxBounds = new Vector3(2f, -0.5f, 0);

        CreateLabel(new Vector3(-2f, -3f, 0), "父旋轉", 22, new Color(0.5f, 0.5f, 0.6f));
        CreateLabel(new Vector3(2f, -3f, 0), "子旋轉", 22, new Color(0.5f, 0.5f, 0.6f));

        matLabel = CreateLabel(new Vector3(0, -1.5f, 0), "", 26, Color.white);
        resultLabel = CreateLabel(new Vector3(0, -2.2f, 0), "", 24, new Color(0.7f, 0.7f, 0.8f));
    }

    public override void UpdateVisualization()
    {
        float parentAngle = Mathf.Lerp(0, 360, Mathf.InverseLerp(-2.5f, -0.5f, handleParentRot.LocalPosition.y));
        float childAngle = Mathf.Lerp(0, 360, Mathf.InverseLerp(-2.5f, -0.5f, handleChildRot.LocalPosition.y));

        parentObj.transform.localRotation = Quaternion.Euler(0, 0, parentAngle);
        childObj.transform.localRotation = Quaternion.Euler(0, 0, childAngle);

        float totalAngle = parentAngle + childAngle;
        matLabel.text = $"Parent {parentAngle:F0}° + Child {childAngle:F0}° = World {totalAngle:F0}°";
        resultLabel.text = "M_world = M_parent × M_local";
    }

    public override bool CheckChallengeComplete()
    {
        float p = Mathf.Lerp(0, 360, Mathf.InverseLerp(-2.5f, -0.5f, handleParentRot.LocalPosition.y));
        float c = Mathf.Lerp(0, 360, Mathf.InverseLerp(-2.5f, -0.5f, handleChildRot.LocalPosition.y));
        return p + c > 180f;
    }
}
