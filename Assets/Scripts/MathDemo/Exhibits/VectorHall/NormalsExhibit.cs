using UnityEngine;

// ============================================================
// NormalsExhibit.cs — 法線概念
// 面法線 vs 頂點法線，Flat vs Smooth shading
// ============================================================

public class NormalsExhibit : ExhibitBase
{
    private DragHandle handleMode;
    private TextMesh modeLabel, infoLabel;
    private GameObject[] triVerts = new GameObject[3];
    private Vector3[] triPos = new Vector3[3];
    private bool smoothMode = false;

    public override void BuildExhibit()
    {
        exhibitName = "法線 Normals";
        description = "法線 = 表面的「朝向」\n\n• Face Normal：一個面一條法線 → Flat Shading\n• Vertex Normal：每頂點平均法線 → Smooth Shading\n\n🎮 遊戲應用：\n• 光照計算核心：diffuse = dot(N, L)\n• 碰撞反應：沿法線方向推出去\n• Backface Culling：法線背對相機的面不畫\n• 地形坡度偵測：角色能否站穩\n\n拖曳切換 Flat / Smooth 模式";
        formula = "N = normalize(AB × AC)";
        challengeDescription = "切換到 Smooth 模式";

        triPos[0] = new Vector3(-1.5f, -0.5f, 0);
        triPos[1] = new Vector3(1.5f, -0.5f, 0);
        triPos[2] = new Vector3(0f, 1.5f, 0);

        for (int i = 0; i < 3; i++)
        {
            triVerts[i] = CreateStaticPrimitive(PrimitiveType.Sphere, triPos[i], Vector3.one * 0.15f, new Color(0.8f, 0.8f, 0.9f));
        }

        handleMode = CreateDragHandle(new Vector3(0, -2f, 0), new Color(0.8f, 0.8f, 0.3f), 0.12f);
        handleMode.minBounds = new Vector3(-1.5f, -2f, 0);
        handleMode.maxBounds = new Vector3(1.5f, -2f, 0);

        CreateLabel(new Vector3(-1.8f, -2f, 0), "Flat", 22, new Color(0.5f, 0.5f, 0.6f));
        CreateLabel(new Vector3(1.8f, -2f, 0), "Smooth", 22, new Color(0.5f, 0.5f, 0.6f));

        modeLabel = CreateLabel(new Vector3(0, -2.8f, 0), "", 30, Color.white);
        infoLabel = CreateLabel(new Vector3(0, -3.4f, 0), "", 24, new Color(0.7f, 0.7f, 0.8f));
    }

    public override void UpdateVisualization()
    {
        var mr = MathLineRenderer.Instance;
        if (mr == null) return;

        smoothMode = handleMode.LocalPosition.x > 0;

        Vector3 a = transform.TransformPoint(triPos[0]);
        Vector3 b = transform.TransformPoint(triPos[1]);
        Vector3 c = transform.TransformPoint(triPos[2]);

        // 畫三角形邊
        mr.DrawLine(a, b, new Color(0.6f, 0.6f, 0.7f), 0.01f);
        mr.DrawLine(b, c, new Color(0.6f, 0.6f, 0.7f), 0.01f);
        mr.DrawLine(c, a, new Color(0.6f, 0.6f, 0.7f), 0.01f);

        Vector3 faceNormal = Vector3.Cross(b - a, c - a).normalized;

        if (smoothMode)
        {
            // Smooth: 每頂點一條法線（模擬平均法線 = 面法線微偏）
            modeLabel.text = "Smooth Shading — 頂點法線";
            infoLabel.text = "每頂點有獨立法線，插值後光照柔和平滑";

            Vector3 center = (a + b + c) / 3f;
            for (int i = 0; i < 3; i++)
            {
                Vector3 v = new[] { a, b, c }[i];
                Vector3 vn = (v - center).normalized * 0.5f + faceNormal * 0.5f;
                vn = vn.normalized;
                mr.DrawArrow(v, v + vn * 0.6f, new Color(0.3f, 1f, 0.5f), 0.012f, 0.06f);
            }
        }
        else
        {
            // Flat: 面中心一條法線
            modeLabel.text = "Flat Shading — 面法線";
            infoLabel.text = "整個面共用一條法線，光照有稜角感";

            Vector3 center = (a + b + c) / 3f;
            mr.DrawArrow(center, center + faceNormal * 0.8f, new Color(1f, 0.5f, 0.3f), 0.015f, 0.08f);
        }
    }

    public override bool CheckChallengeComplete() => smoothMode;
}
