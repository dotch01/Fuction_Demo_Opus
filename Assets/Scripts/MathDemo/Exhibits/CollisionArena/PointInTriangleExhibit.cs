using UnityEngine;

// ============================================================
// PointInTriangleExhibit.cs — 點在三角形內判定
// 拖動一點，三角形即時變色表示在內/在外
// 同時顯示面積法和叉積法的結果
// ============================================================

public class PointInTriangleExhibit : ExhibitBase
{
    private DragHandle handlePoint;
    private DragHandle handleA, handleB, handleC;
    private TextMesh resultLabel;
    private TextMesh methodLabel;

    public override void BuildExhibit()
    {
        exhibitName = "點在三角形內 Point-in-Triangle";
        description = "兩種判定方法：\n\n① 面積法：子三角形面積之和 = 原三角形面積\n② 叉積法：P 在所有邊的同一側\n   → sign(AB×AP) = sign(BC×BP) = sign(CA×CP)\n\n🎮 遊戲應用：\n• 3D 滑鼠點選：射線打到三角面後判斷命中\n• 地形取樣：站在哪個三角形上\n• 光柵化基礎：像素是否在三角形內\n\n綠色 = 在內，紅色 = 在外\n拖曳白球測試不同位置";
        formula = "sign(cross(B-A, P-A)) = sign(cross(C-B, P-B)) = sign(cross(A-C, P-C))";
        challengeDescription = "把白點精確放到三角形重心位置";

        // 三角形頂點
        handleA = CreateDragHandle(new Vector3(-2, -1.5f, 0), new Color(1f, 0.4f, 0.4f), 0.13f);
        handleB = CreateDragHandle(new Vector3(2, -1.5f, 0), new Color(0.4f, 0.4f, 1f), 0.13f);
        handleC = CreateDragHandle(new Vector3(0, 2, 0), new Color(0.4f, 1f, 0.4f), 0.13f);

        CreateLabel(new Vector3(-2.3f, -2f, 0), "A", 25, new Color(1f, 0.4f, 0.4f));
        CreateLabel(new Vector3(2.2f, -2f, 0), "B", 25, new Color(0.4f, 0.4f, 1f));
        CreateLabel(new Vector3(0, 2.5f, 0), "C", 25, new Color(0.4f, 1f, 0.4f));

        // 測試點
        handlePoint = CreateDragHandle(new Vector3(0.3f, 0, 0), Color.white, 0.2f);

        resultLabel = CreateLabel(new Vector3(0, -2.8f, 0), "", 36, Color.white);
        methodLabel = CreateLabel(new Vector3(0, -3.5f, 0), "", 24, new Color(0.7f, 0.7f, 0.8f));
    }

    public override void UpdateVisualization()
    {
        if (MathLineRenderer.Instance == null) return;
        var mr = MathLineRenderer.Instance;

        Vector3 a = handleA.LocalPosition;
        Vector3 b = handleB.LocalPosition;
        Vector3 c = handleC.LocalPosition;
        Vector3 p = handlePoint.LocalPosition;

        // 叉積判定
        float d1 = Cross2D(b - a, p - a);
        float d2 = Cross2D(c - b, p - b);
        float d3 = Cross2D(a - c, p - c);

        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
        bool inside = !(hasNeg && hasPos);

        Color triColor = inside ? new Color(0.2f, 0.8f, 0.3f, 0.6f) : new Color(0.8f, 0.2f, 0.2f, 0.4f);

        // 三角形邊
        mr.DrawLine(transform.TransformPoint(a), transform.TransformPoint(b), triColor, 0.03f);
        mr.DrawLine(transform.TransformPoint(b), transform.TransformPoint(c), triColor, 0.03f);
        mr.DrawLine(transform.TransformPoint(c), transform.TransformPoint(a), triColor, 0.03f);

        // 面積法計算
        float totalArea = Mathf.Abs(Cross2D(b - a, c - a)) * 0.5f;
        float area1 = Mathf.Abs(Cross2D(b - p, c - p)) * 0.5f;
        float area2 = Mathf.Abs(Cross2D(c - p, a - p)) * 0.5f;
        float area3 = Mathf.Abs(Cross2D(a - p, b - p)) * 0.5f;

        // P 到各頂點的虛線
        mr.DrawDashedLine(transform.TransformPoint(p), transform.TransformPoint(a), new Color(1f, 1f, 1f, 0.2f));
        mr.DrawDashedLine(transform.TransformPoint(p), transform.TransformPoint(b), new Color(1f, 1f, 1f, 0.2f));
        mr.DrawDashedLine(transform.TransformPoint(p), transform.TransformPoint(c), new Color(1f, 1f, 1f, 0.2f));

        // 重心
        Vector3 centroid = (a + b + c) / 3f;
        mr.DrawPoint(transform.TransformPoint(centroid), new Color(1f, 0.9f, 0.3f), 0.06f);

        resultLabel.text = inside ? "✓ 點在三角形內！" : "✗ 點在三角形外";
        resultLabel.color = inside ? new Color(0.3f, 1f, 0.4f) : new Color(1f, 0.3f, 0.3f);

        methodLabel.text = $"面積法：{area1:F2} + {area2:F2} + {area3:F2} = {(area1 + area2 + area3):F2}（原面積 = {totalArea:F2}）";
    }

    private float Cross2D(Vector3 a, Vector3 b) => a.x * b.y - a.y * b.x;

    public override bool CheckChallengeComplete()
    {
        Vector3 centroid = (handleA.LocalPosition + handleB.LocalPosition + handleC.LocalPosition) / 3f;
        return Vector3.Distance(handlePoint.LocalPosition, centroid) < 0.3f;
    }

    protected override void OnChallengeCompleted()
    {
        base.OnChallengeCompleted();
        ChallengeSystem.Instance?.MarkCompleted(exhibitName);
    }
}
