using UnityEngine;

// ============================================================
// FrustumCullingExhibit.cs — 視錐剔除
// 可移動的相機視錐，被剔除的物體變灰
// ============================================================

public class FrustumCullingExhibit : ExhibitBase
{
    private DragHandle handleFrustumPos;
    private DragHandle handleFrustumDir;
    private GameObject[] sceneObjects;
    private Renderer[] sceneRenderers;
    private TextMesh infoLabel;

    private float fov = 60f;
    private float nearPlane = 0.5f;
    private float farPlane = 6f;

    public override void BuildExhibit()
    {
        exhibitName = "Frustum Culling 視錐剔除";
        description = "只渲染相機看得到的物體！\n\n視錐體 = 6 個平面（近/遠/上/下/左/右）\n物體的 AABB 與 6 個平面測試：\n  → 全在外面 = 剔除（灰色）\n  → 繪製（彩色）\n\n🎮 遊戲應用：\n• Unity 自動執行視錐剔除\n• 開放世界效能的第一道防線\n• Occlusion Culling 進一步剔除遮擋物\n• 1000 個物件只畫看得到的 100 個\n\n拖曳改變視錐位置和方向";
        formula = "dot(plane.normal, point) + plane.d > 0 ? inside : outside";
        challengeDescription = "移動視錐讓恰好 3 個物體在內";

        // 場景中的小物件
        sceneObjects = new GameObject[8];
        sceneRenderers = new Renderer[8];
        Color[] colors = {
            new Color(1f, 0.3f, 0.3f), new Color(0.3f, 1f, 0.3f),
            new Color(0.3f, 0.3f, 1f), new Color(1f, 1f, 0.3f),
            new Color(1f, 0.3f, 1f), new Color(0.3f, 1f, 1f),
            new Color(1f, 0.7f, 0.3f), new Color(0.7f, 0.3f, 1f)
        };

        for (int i = 0; i < 8; i++)
        {
            float angle = (float)i / 8 * Mathf.PI * 2f;
            float radius = 2.5f + (i % 3) * 0.5f;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0.3f + (i % 2) * 0.5f, Mathf.Sin(angle) * radius * 0.3f);

            PrimitiveType type = (i % 3 == 0) ? PrimitiveType.Cube :
                                 (i % 3 == 1) ? PrimitiveType.Sphere : PrimitiveType.Cylinder;

            sceneObjects[i] = CreateStaticPrimitive(type, pos, Vector3.one * 0.5f, colors[i]);
            sceneRenderers[i] = sceneObjects[i].GetComponent<Renderer>();
        }

        // 視錐控制
        handleFrustumPos = CreateDragHandle(new Vector3(-3, 0, 0), new Color(1f, 0.85f, 0.2f), 0.17f);
        handleFrustumDir = CreateDragHandle(new Vector3(0, 0, 0), new Color(1f, 0.6f, 0.2f), 0.12f);

        infoLabel = CreateLabel(new Vector3(0, -2.5f, 0), "", 32, Color.white);
    }

    public override void UpdateVisualization()
    {
        if (MathLineRenderer.Instance == null) return;
        var mr = MathLineRenderer.Instance;

        Vector3 frustumPos = handleFrustumPos.LocalPosition;
        Vector3 frustumDir = (handleFrustumDir.LocalPosition - frustumPos).normalized;
        if (frustumDir.sqrMagnitude < 0.001f) frustumDir = Vector3.right;

        // 計算簡化的 2D 視錐（兩條射線形成三角形）
        float halfAngle = fov * 0.5f * Mathf.Deg2Rad;
        Vector3 perpDir = new Vector3(-frustumDir.y, frustumDir.x, 0);

        Vector3 nearCenter = frustumPos + frustumDir * nearPlane;
        Vector3 farCenter = frustumPos + frustumDir * farPlane;
        float nearHalf = Mathf.Tan(halfAngle) * nearPlane;
        float farHalf = Mathf.Tan(halfAngle) * farPlane;

        Vector3 nTL = nearCenter + perpDir * nearHalf;
        Vector3 nBL = nearCenter - perpDir * nearHalf;
        Vector3 fTL = farCenter + perpDir * farHalf;
        Vector3 fBL = farCenter - perpDir * farHalf;

        // 畫視錐
        Color frustumColor = new Color(1f, 0.85f, 0.2f, 0.5f);
        mr.DrawLine(transform.TransformPoint(nTL), transform.TransformPoint(fTL), frustumColor, 0.015f);
        mr.DrawLine(transform.TransformPoint(nBL), transform.TransformPoint(fBL), frustumColor, 0.015f);
        mr.DrawLine(transform.TransformPoint(nTL), transform.TransformPoint(nBL), frustumColor, 0.015f);
        mr.DrawLine(transform.TransformPoint(fTL), transform.TransformPoint(fBL), frustumColor, 0.015f);
        mr.DrawLine(transform.TransformPoint(frustumPos), transform.TransformPoint(nTL), new Color(1f, 0.85f, 0.2f, 0.3f), 0.008f);
        mr.DrawLine(transform.TransformPoint(frustumPos), transform.TransformPoint(nBL), new Color(1f, 0.85f, 0.2f, 0.3f), 0.008f);

        // 測試每個物件
        int visibleCount = 0;
        for (int i = 0; i < sceneObjects.Length; i++)
        {
            Vector3 objPos = sceneObjects[i].transform.localPosition;
            bool inside = IsInFrustum(objPos, frustumPos, frustumDir, perpDir, halfAngle);

            if (inside)
            {
                visibleCount++;
                sceneRenderers[i].material.color = Color.HSVToRGB((float)i / 8, 0.7f, 0.9f);
            }
            else
            {
                sceneRenderers[i].material.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            }
        }

        infoLabel.text = $"可見物體：{visibleCount} / {sceneObjects.Length}    FOV = {fov}°";
        infoLabel.color = visibleCount == 3 ? new Color(0.3f, 1f, 0.5f) : Color.white;
    }

    private bool IsInFrustum(Vector3 point, Vector3 origin, Vector3 forward, Vector3 perp, float halfAngle)
    {
        Vector3 toPoint = point - origin;
        float forwardDist = Vector3.Dot(toPoint, forward);

        if (forwardDist < nearPlane || forwardDist > farPlane) return false;

        float lateralDist = Mathf.Abs(Vector3.Dot(toPoint, perp));
        float maxLateral = Mathf.Tan(halfAngle) * forwardDist;

        return lateralDist <= maxLateral;
    }

    public override bool CheckChallengeComplete()
    {
        Vector3 frustumPos = handleFrustumPos.LocalPosition;
        Vector3 frustumDir = (handleFrustumDir.LocalPosition - frustumPos).normalized;
        if (frustumDir.sqrMagnitude < 0.001f) return false;

        float halfAngle = fov * 0.5f * Mathf.Deg2Rad;
        Vector3 perpDir = new Vector3(-frustumDir.y, frustumDir.x, 0);

        int count = 0;
        for (int i = 0; i < sceneObjects.Length; i++)
        {
            if (IsInFrustum(sceneObjects[i].transform.localPosition, frustumPos, frustumDir, perpDir, halfAngle))
                count++;
        }
        return count == 3;
    }

    protected override void OnChallengeCompleted()
    {
        base.OnChallengeCompleted();
        ChallengeSystem.Instance?.MarkCompleted(exhibitName);
    }
}
