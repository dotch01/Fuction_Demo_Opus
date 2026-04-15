using UnityEngine;

// ============================================================
// Atan2Exhibit.cs — 反三角函數 atan2
// 拖點顯示 atan2(y,x) 的角度
// ============================================================

public class Atan2Exhibit : ExhibitBase
{
    private DragHandle handlePoint;
    private TextMesh angleLabel, quadrantLabel, infoLabel;

    public override void BuildExhibit()
    {
        exhibitName = "atan2 反三角函數";
        description = "atan2(y, x) = 從 X 軸到 (x,y) 的角度\n\n範圍：(-π, π] = (-180°, 180°]\n\n比 atan(y/x) 好！\n• 處理所有四個象限\n• x=0 不會除以零\n\n🎮 遊戲應用：\n• 2D 角色朝向敵人的角度\n• 砲塔自動旋轉瞥準\n• 搖桿/觸控方向轉為角度\n• 小地圖上目標方向箭頭\n\n拖曳點觀察角度變化";
        formula = "θ = atan2(y, x) ∈ (-π, π]";
        challengeDescription = "把點拖到第三象限 (x<0, y<0)";

        handlePoint = CreateDragHandle(new Vector3(1.5f, 1f, 0), new Color(1f, 0.5f, 0.3f));

        // 座標軸
        CreateStaticPrimitive(PrimitiveType.Cube, new Vector3(0, 0, 0), new Vector3(4f, 0.02f, 0.02f), new Color(0.3f, 0.3f, 0.4f));
        CreateStaticPrimitive(PrimitiveType.Cube, new Vector3(0, 0, 0), new Vector3(0.02f, 4f, 0.02f), new Color(0.3f, 0.3f, 0.4f));

        angleLabel = CreateLabel(new Vector3(0, -2f, 0), "", 32, Color.white);
        quadrantLabel = CreateLabel(new Vector3(0, -2.6f, 0), "", 28, new Color(1f, 0.85f, 0.3f));
        infoLabel = CreateLabel(new Vector3(0, -3.2f, 0), "", 24, new Color(0.7f, 0.7f, 0.8f));
    }

    public override void UpdateVisualization()
    {
        var mr = MathLineRenderer.Instance;
        if (mr == null) return;

        Vector3 p = handlePoint.LocalPosition;
        float angle = Mathf.Atan2(p.y, p.x);
        float degrees = angle * Mathf.Rad2Deg;

        // 原點到點的線
        Vector3 origin = transform.position;
        Vector3 pw = transform.TransformPoint(p);
        mr.DrawArrow(origin, pw, new Color(1f, 0.5f, 0.3f), 0.012f, 0.06f);

        // 畫角度弧
        mr.DrawArc(origin, transform.right, angle * Mathf.Rad2Deg, 0.6f, new Color(0.3f, 0.9f, 0.5f), 0.01f, 24);

        // 象限
        string quad;
        if (p.x >= 0 && p.y >= 0) quad = "第一象限 Q1 (0° ~ 90°)";
        else if (p.x < 0 && p.y >= 0) quad = "第二象限 Q2 (90° ~ 180°)";
        else if (p.x < 0 && p.y < 0) quad = "第三象限 Q3 (-180° ~ -90°)";
        else quad = "第四象限 Q4 (-90° ~ 0°)";

        angleLabel.text = $"atan2({p.y:F2}, {p.x:F2}) = {degrees:F1}° ({angle:F3} rad)";
        quadrantLabel.text = quad;
        infoLabel.text = "全象限安全的角度計算！";
    }

    public override bool CheckChallengeComplete()
    {
        return handlePoint.LocalPosition.x < 0 && handlePoint.LocalPosition.y < 0;
    }
}
