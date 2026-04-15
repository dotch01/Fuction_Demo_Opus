using UnityEngine;

// ============================================================
// AABBExhibit.cs — AABB 碰撞偵測
// 拖動兩個方塊，碰到時變色
// ============================================================

public class AABBExhibit : ExhibitBase
{
    private DragHandle handleBoxA;
    private DragHandle handleBoxB;
    private GameObject boxVisA;
    private GameObject boxVisB;
    private TextMesh resultLabel;
    private TextMesh detailLabel;

    private Vector3 sizeA = new Vector3(1.5f, 1.2f, 0.5f);
    private Vector3 sizeB = new Vector3(1.2f, 1f, 0.5f);

    public override void BuildExhibit()
    {
        exhibitName = "AABB 碰撞 Axis-Aligned Bounding Box";
        description = "最快的碰撞偵測之一！\n\nAABB = 軸對齊的包圍盒\n碰撞條件：3 個軸都重疊\n  minA.x ≤ maxB.x && maxA.x ≥ minB.x\n  (Y 和 Z 同理)\n\n🎮 遊戲應用：\n• 物理引擎 Broad Phase 粗篩\n• 空間分割（Octree/BVH）的節點\n• UI 按鈕點擊判定\n• 只需 6 次比較，極度高效！\n\n拖曳兩方塊觀察碰撞";
        formula = "overlap = (minA ≤ maxB) && (maxA ≥ minB) 每軸";
        challengeDescription = "讓兩方塊恰好邊緣接觸（重疊面積 < 0.1）";

        handleBoxA = CreateDragHandle(new Vector3(-2, 0, 0), new Color(0.3f, 0.5f, 1f), 0.13f);
        handleBoxB = CreateDragHandle(new Vector3(2, 0, 0), new Color(1f, 0.5f, 0.3f), 0.13f);

        boxVisA = CreateStaticPrimitive(PrimitiveType.Cube, Vector3.zero, sizeA, new Color(0.3f, 0.5f, 1f, 0.6f));
        boxVisB = CreateStaticPrimitive(PrimitiveType.Cube, Vector3.zero, sizeB, new Color(1f, 0.5f, 0.3f, 0.6f));

        resultLabel = CreateLabel(new Vector3(0, -2.5f, 0), "", 36, Color.white);
        detailLabel = CreateLabel(new Vector3(0, -3.2f, 0), "", 22, new Color(0.7f, 0.7f, 0.8f));
    }

    public override void UpdateVisualization()
    {
        if (MathLineRenderer.Instance == null) return;
        var mr = MathLineRenderer.Instance;

        Vector3 posA = handleBoxA.LocalPosition;
        Vector3 posB = handleBoxB.LocalPosition;

        boxVisA.transform.localPosition = posA;
        boxVisB.transform.localPosition = posB;

        Vector3 minA = posA - sizeA * 0.5f;
        Vector3 maxA = posA + sizeA * 0.5f;
        Vector3 minB = posB - sizeB * 0.5f;
        Vector3 maxB = posB + sizeB * 0.5f;

        bool overlapX = minA.x <= maxB.x && maxA.x >= minB.x;
        bool overlapY = minA.y <= maxB.y && maxA.y >= minB.y;
        bool overlapZ = minA.z <= maxB.z && maxA.z >= minB.z;
        bool colliding = overlapX && overlapY && overlapZ;

        // AABB 框線
        Color colorA = colliding ? new Color(1f, 0.3f, 0.3f) : new Color(0.3f, 0.5f, 1f);
        Color colorB = colliding ? new Color(1f, 0.3f, 0.3f) : new Color(1f, 0.5f, 0.3f);

        mr.DrawAABB(transform.TransformPoint(minA), transform.TransformPoint(maxA), colorA, 0.02f);
        mr.DrawAABB(transform.TransformPoint(minB), transform.TransformPoint(maxB), colorB, 0.02f);

        // 重疊區域
        if (colliding)
        {
            Vector3 overlapMin = Vector3.Max(minA, minB);
            Vector3 overlapMax = Vector3.Min(maxA, maxB);
            mr.DrawAABB(transform.TransformPoint(overlapMin), transform.TransformPoint(overlapMax),
                        new Color(1f, 1f, 0.3f), 0.025f);
        }

        // 每軸重疊投影
        float yLine = -1.8f;
        // X 軸
        mr.DrawLine(transform.TransformPoint(new Vector3(minA.x, yLine, 0)),
                    transform.TransformPoint(new Vector3(maxA.x, yLine, 0)),
                    overlapX ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 0.3f, 0.3f), 0.02f);
        mr.DrawLine(transform.TransformPoint(new Vector3(minB.x, yLine - 0.15f, 0)),
                    transform.TransformPoint(new Vector3(maxB.x, yLine - 0.15f, 0)),
                    overlapX ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 0.3f, 0.3f), 0.02f);

        boxVisA.GetComponent<Renderer>().material.color = colliding ?
            new Color(1f, 0.3f, 0.3f, 0.6f) : new Color(0.3f, 0.5f, 1f, 0.6f);
        boxVisB.GetComponent<Renderer>().material.color = colliding ?
            new Color(1f, 0.3f, 0.3f, 0.6f) : new Color(1f, 0.5f, 0.3f, 0.6f);

        resultLabel.text = colliding ? "✓ 碰撞！COLLISION!" : "✗ 未碰撞";
        resultLabel.color = colliding ? new Color(1f, 0.3f, 0.3f) : new Color(0.5f, 0.8f, 0.5f);

        detailLabel.text = $"X: {(overlapX ? "✓" : "✗")}    Y: {(overlapY ? "✓" : "✗")}    Z: {(overlapZ ? "✓" : "✗")}    （三軸都要重疊才碰撞）";
    }

    public override bool CheckChallengeComplete()
    {
        Vector3 posA = handleBoxA.LocalPosition;
        Vector3 posB = handleBoxB.LocalPosition;
        Vector3 minA = posA - sizeA * 0.5f;
        Vector3 maxA = posA + sizeA * 0.5f;
        Vector3 minB = posB - sizeB * 0.5f;
        Vector3 maxB = posB + sizeB * 0.5f;

        bool overlapX = minA.x <= maxB.x && maxA.x >= minB.x;
        bool overlapY = minA.y <= maxB.y && maxA.y >= minB.y;
        bool colliding = overlapX && overlapY;

        if (!colliding) return false;

        Vector3 overlapMin = Vector3.Max(minA, minB);
        Vector3 overlapMax = Vector3.Min(maxA, maxB);
        float overlapArea = (overlapMax.x - overlapMin.x) * (overlapMax.y - overlapMin.y);

        return overlapArea > 0 && overlapArea < 0.1f;
    }

    protected override void OnChallengeCompleted()
    {
        base.OnChallengeCompleted();
        ChallengeSystem.Instance?.MarkCompleted(exhibitName);
    }
}
