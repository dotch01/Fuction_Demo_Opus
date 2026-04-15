using UnityEngine;

// ============================================================
// RayTriangleExhibit.cs — 射線三角形相交
// Möller–Trumbore 演算法視覺化
// ============================================================

public class RayTriangleExhibit : ExhibitBase
{
    private DragHandle handleRayOrigin;
    private DragHandle handleRayDir;
    private TextMesh resultLabel;
    private TextMesh tLabel;
    private GameObject hitMarker;

    // 固定三角形
    private Vector3 triA = new Vector3(-2, -1.5f, 0);
    private Vector3 triB = new Vector3(2, -1.5f, 0);
    private Vector3 triC = new Vector3(0, 2, 0);

    public override void BuildExhibit()
    {
        exhibitName = "射線三角形相交 Ray-Triangle";
        description = "Möller–Trumbore 演算法\n\n① 射線 R(t) = O + t × D\n② 解 baryCentric 座標 (u,v)\n③ 若 t>0 且 u,v≥0 且 u+v≤1 → 碰到！\n\n🎮 遊戲應用：\n• Physics.Raycast() 的底層數學\n• 滑鼠點選 3D 物件（Screen → Ray → Mesh）\n• 子彈命中判定\n• 光線追蹤的核心運算\n\n拖曳射線起點和方向觀察碰撞";
        formula = "O + tD = (1-u-v)A + uB + vC";
        challengeDescription = "讓射線準確命中三角形中心";

        // 三角形頂點（靜態）
        CreateStaticPrimitive(PrimitiveType.Sphere, triA, Vector3.one * 0.1f, new Color(1f, 0.4f, 0.4f));
        CreateStaticPrimitive(PrimitiveType.Sphere, triB, Vector3.one * 0.1f, new Color(0.4f, 0.4f, 1f));
        CreateStaticPrimitive(PrimitiveType.Sphere, triC, Vector3.one * 0.1f, new Color(0.4f, 1f, 0.4f));

        // 射線控制
        handleRayOrigin = CreateDragHandle(new Vector3(-3.5f, 0, 0), new Color(1f, 0.7f, 0.2f), 0.17f);
        handleRayDir = CreateDragHandle(new Vector3(-1.5f, 0.5f, 0), new Color(1f, 0.9f, 0.5f), 0.13f);

        hitMarker = CreateStaticPrimitive(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.2f, new Color(1f, 0.3f, 1f));
        hitMarker.SetActive(false);

        resultLabel = CreateLabel(new Vector3(0, -2.5f, 0), "", 34, Color.white);
        tLabel = CreateLabel(new Vector3(0, -3.2f, 0), "", 28, new Color(0.7f, 0.7f, 0.8f));
    }

    public override void UpdateVisualization()
    {
        if (MathLineRenderer.Instance == null) return;
        var mr = MathLineRenderer.Instance;

        Vector3 rayOrigin = handleRayOrigin.LocalPosition;
        Vector3 rayDir = (handleRayDir.LocalPosition - rayOrigin).normalized;

        // 三角形
        Color triBaseColor = new Color(0.4f, 0.5f, 0.7f);
        float t, u, v;
        bool hit = RayTriangleIntersect(rayOrigin, rayDir, triA, triB, triC, out t, out u, out v);

        Color triColor = hit ? new Color(0.3f, 1f, 0.4f) : triBaseColor;
        mr.DrawLine(transform.TransformPoint(triA), transform.TransformPoint(triB), triColor, 0.025f);
        mr.DrawLine(transform.TransformPoint(triB), transform.TransformPoint(triC), triColor, 0.025f);
        mr.DrawLine(transform.TransformPoint(triC), transform.TransformPoint(triA), triColor, 0.025f);

        // 射線
        Vector3 rayEnd = rayOrigin + rayDir * 10f;
        mr.DrawArrow(transform.TransformPoint(rayOrigin), transform.TransformPoint(rayOrigin + rayDir * 5f),
                     new Color(1f, 0.7f, 0.2f), 0.025f);

        if (hit && t > 0)
        {
            Vector3 hitPoint = rayOrigin + rayDir * t;
            hitMarker.SetActive(true);
            hitMarker.transform.localPosition = hitPoint;

            // 碰撞點到各頂點的虛線
            mr.DrawDashedLine(transform.TransformPoint(hitPoint), transform.TransformPoint(triA), new Color(1f, 1f, 1f, 0.2f));
            mr.DrawDashedLine(transform.TransformPoint(hitPoint), transform.TransformPoint(triB), new Color(1f, 1f, 1f, 0.2f));
            mr.DrawDashedLine(transform.TransformPoint(hitPoint), transform.TransformPoint(triC), new Color(1f, 1f, 1f, 0.2f));

            resultLabel.text = "✓ 碰到！HIT!";
            resultLabel.color = new Color(0.3f, 1f, 0.4f);
            tLabel.text = $"t = {t:F2}    u = {u:F3}    v = {v:F3}    (1-u-v) = {1 - u - v:F3}";
        }
        else
        {
            hitMarker.SetActive(false);
            resultLabel.text = "✗ 未碰到 MISS";
            resultLabel.color = new Color(1f, 0.4f, 0.3f);
            tLabel.text = hit ? $"t = {t:F2}（在射線反方向）" : "射線與三角形平行或不交";
        }
    }

    // Möller–Trumbore
    private bool RayTriangleIntersect(Vector3 orig, Vector3 dir, Vector3 v0, Vector3 v1, Vector3 v2,
                                       out float t, out float u, out float v)
    {
        t = u = v = 0;
        float eps = 0.000001f;

        Vector3 e1 = v1 - v0;
        Vector3 e2 = v2 - v0;
        Vector3 h = Vector3.Cross(dir, e2);
        float a = Vector3.Dot(e1, h);

        if (a > -eps && a < eps) return false;

        float f = 1f / a;
        Vector3 s = orig - v0;
        u = f * Vector3.Dot(s, h);
        if (u < 0f || u > 1f) return false;

        Vector3 q = Vector3.Cross(s, e1);
        v = f * Vector3.Dot(dir, q);
        if (v < 0f || u + v > 1f) return false;

        t = f * Vector3.Dot(e2, q);
        return true;
    }

    public override bool CheckChallengeComplete()
    {
        Vector3 rayOrigin = handleRayOrigin.LocalPosition;
        Vector3 rayDir = (handleRayDir.LocalPosition - rayOrigin).normalized;
        float t, u, v;
        bool hit = RayTriangleIntersect(rayOrigin, rayDir, triA, triB, triC, out t, out u, out v);
        // 中心 = u=1/3, v=1/3
        return hit && t > 0 && Mathf.Abs(u - 0.333f) < 0.08f && Mathf.Abs(v - 0.333f) < 0.08f;
    }

    protected override void OnChallengeCompleted()
    {
        base.OnChallengeCompleted();
        ChallengeSystem.Instance?.MarkCompleted(exhibitName);
    }
}
