using UnityEngine;

// ============================================================
// CausticsExhibit.cs — 焦散
// 平行光線射入弧面，折射匯聚
// ============================================================

public class CausticsExhibit : ExhibitBase
{
    private DragHandle handleCurve;
    private TextMesh infoLabel, focusLabel;
    private int rayCount = 12;

    public override void BuildExhibit()
    {
        exhibitName = "焦散 Caustics";
        description = "平行光線射入弧面鏡 → 反射匯聚點\n\n焦點 f = R/2（拋物面精確、球面近似）\n\n🎮 遊戲應用：\n• 水面光斑效果（泳池底部的波紋光）\n• 玻璃/水晶材質的折射集中\n• 車漆/寶石的高光散射\n• URP/HDRP 焦散渲染效果\n\n拖曳控制弧面曲率觀察匯聚";
        formula = "f = R/2    反射: R = I - 2(I·N)N";
        challengeDescription = "讓光線匯聚到清晰焦點";

        handleCurve = CreateDragHandle(new Vector3(-2.5f, 0, 0), new Color(0.3f, 0.8f, 1f), 0.12f);
        handleCurve.minBounds = new Vector3(-2.5f, -1.5f, 0);
        handleCurve.maxBounds = new Vector3(-2.5f, 1.5f, 0);

        CreateLabel(new Vector3(-2.5f, -2f, 0), "曲率", 20, new Color(0.5f, 0.5f, 0.6f));
        infoLabel = CreateLabel(new Vector3(0, -2.5f, 0), "", 26, Color.white);
        focusLabel = CreateLabel(new Vector3(0, -3.1f, 0), "", 24, new Color(0.7f, 0.7f, 0.8f));
    }

    public override void UpdateVisualization()
    {
        var mr = MathLineRenderer.Instance;
        if (mr == null) return;

        float curvature = Mathf.Lerp(0.3f, 3f, Mathf.InverseLerp(-1.5f, 1.5f, handleCurve.LocalPosition.y));
        float R = 1f / curvature;
        float mirrorX = -1.5f;

        // 畫弧面鏡
        int arcSteps = 30;
        float arcHalf = 1.5f;
        for (int i = 0; i < arcSteps; i++)
        {
            float y0 = Mathf.Lerp(-arcHalf, arcHalf, (float)i / arcSteps);
            float y1 = Mathf.Lerp(-arcHalf, arcHalf, (float)(i + 1) / arcSteps);
            float x0 = mirrorX + (y0 * y0) / (2f * R);
            float x1 = mirrorX + (y1 * y1) / (2f * R);
            mr.DrawLine(transform.TransformPoint(new Vector3(x0, y0, 0)),
                        transform.TransformPoint(new Vector3(x1, y1, 0)),
                        new Color(0.5f, 0.5f, 0.6f), 0.015f);
        }

        // 平行光線 → 反射
        float focus = R / 2f;
        for (int i = 0; i < rayCount; i++)
        {
            float y = Mathf.Lerp(-arcHalf * 0.8f, arcHalf * 0.8f, (float)i / (rayCount - 1));
            float hitX = mirrorX + (y * y) / (2f * R);

            // 法線（拋物面法線）
            float nx = 1f;
            float ny = -y / R;
            Vector2 normal = new Vector2(nx, ny).normalized;

            // 入射光（向左）
            Vector2 incident = new Vector2(-1, 0);
            Vector2 reflected = incident - 2 * Vector2.Dot(incident, normal) * normal;

            Vector3 hitPos = new Vector3(hitX, y, 0);
            Vector3 rayStart = new Vector3(hitX + 2f, y, 0);

            // 入射線
            mr.DrawLine(transform.TransformPoint(rayStart), transform.TransformPoint(hitPos),
                new Color(1f, 0.9f, 0.5f, 0.5f), 0.006f);

            // 反射線
            Vector3 refEnd = hitPos + new Vector3(reflected.x, reflected.y, 0) * 3f;
            mr.DrawLine(transform.TransformPoint(hitPos), transform.TransformPoint(refEnd),
                new Color(1f, 0.7f, 0.2f), 0.008f);
        }

        // 焦點標記
        Vector3 focalPoint = new Vector3(mirrorX + focus, 0, 0);
        mr.DrawLine(transform.TransformPoint(focalPoint - Vector3.up * 0.1f),
                    transform.TransformPoint(focalPoint + Vector3.up * 0.1f),
                    new Color(1f, 0.3f, 0.3f), 0.04f);

        infoLabel.text = $"曲率 κ = {curvature:F2}    R = {R:F2}    焦距 f = {focus:F2}";
        focusLabel.text = "光線匯聚到焦點 → 焦散效果";
    }

    public override bool CheckChallengeComplete()
    {
        float curvature = Mathf.Lerp(0.3f, 3f, Mathf.InverseLerp(-1.5f, 1.5f, handleCurve.LocalPosition.y));
        return curvature > 1f && curvature < 2f;
    }
}
