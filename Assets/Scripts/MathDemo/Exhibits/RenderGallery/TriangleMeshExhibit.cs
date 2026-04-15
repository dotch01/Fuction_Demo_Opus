using UnityEngine;

// ============================================================
// TriangleMeshExhibit.cs — 三角網格
// 顯示 Mesh 的頂點/邊/面
// ============================================================

public class TriangleMeshExhibit : ExhibitBase
{
    private DragHandle handleMode;
    private TextMesh modeLabel, statsLabel;
    private Vector3[] verts;
    private int[] tris;
    private bool wireframe = false;

    public override void BuildExhibit()
    {
        exhibitName = "三角網格 Triangle Mesh";
        description = "3D 圖形由三角形組成：\n\n• 頂點 (Vertices)：空間中的點\n• 三角面 (Triangles)：每 3 個頂點一面\n• 邊 (Edges)：頂點間的連線\n\n🎮 遊戲應用：\n• 所有 3D 模型的基礎結構\n• LOD：遠處減少三角形數量\n• 程式生成 Mesh（地形、水面）\n• Wireframe 除錯模式\n\n拖曳切換 Wireframe / Solid";
        formula = "面數 F, 邊數 E, 頂點數 V: V-E+F=2 (Euler)";
        challengeDescription = "切換到 Wireframe 模式";

        // 簡單的金字塔 mesh
        verts = new[] {
            new Vector3(0, 1.5f, 0),      // top
            new Vector3(-1f, -0.5f, -0.5f), // front-left
            new Vector3(1f, -0.5f, -0.5f),  // front-right
            new Vector3(1f, -0.5f, 0.5f),   // back-right
            new Vector3(-1f, -0.5f, 0.5f),  // back-left
        };

        tris = new[] {
            0,1,2, 0,2,3, 0,3,4, 0,4,1, // sides
            1,3,2, 1,4,3                  // bottom
        };

        handleMode = CreateDragHandle(new Vector3(0, -2f, 0), new Color(0.8f, 0.8f, 0.3f), 0.12f);
        handleMode.minBounds = new Vector3(-1.5f, -2f, 0);
        handleMode.maxBounds = new Vector3(1.5f, -2f, 0);

        CreateLabel(new Vector3(-1.8f, -2f, 0), "Solid", 20, new Color(0.5f, 0.5f, 0.6f));
        CreateLabel(new Vector3(1.8f, -2f, 0), "Wire", 20, new Color(0.5f, 0.5f, 0.6f));

        modeLabel = CreateLabel(new Vector3(0, -2.7f, 0), "", 28, Color.white);
        statsLabel = CreateLabel(new Vector3(0, -3.3f, 0), $"V={verts.Length} E=8 F={tris.Length / 3} → Euler: {verts.Length}-8+{tris.Length / 3}=2 ✓", 22, new Color(0.7f, 0.7f, 0.8f));
    }

    public override void UpdateVisualization()
    {
        var mr = MathLineRenderer.Instance;
        if (mr == null) return;

        wireframe = handleMode.LocalPosition.x > 0;
        float rotY = Time.time * 20f;

        // 旋轉頂點
        Quaternion rot = Quaternion.Euler(0, rotY, 0);
        Vector3[] rv = new Vector3[verts.Length];
        for (int i = 0; i < verts.Length; i++)
            rv[i] = transform.TransformPoint(rot * verts[i]);

        if (wireframe)
        {
            // 所有邊
            for (int i = 0; i < tris.Length; i += 3)
            {
                mr.DrawLine(rv[tris[i]], rv[tris[i + 1]], new Color(0.3f, 0.8f, 1f), 0.008f);
                mr.DrawLine(rv[tris[i + 1]], rv[tris[i + 2]], new Color(0.3f, 0.8f, 1f), 0.008f);
                mr.DrawLine(rv[tris[i + 2]], rv[tris[i]], new Color(0.3f, 0.8f, 1f), 0.008f);
            }

            // 頂點球
            for (int i = 0; i < rv.Length; i++)
                mr.DrawLine(rv[i] - Vector3.right * 0.02f, rv[i] + Vector3.right * 0.02f, Color.yellow, 0.04f);

            modeLabel.text = "Wireframe — 只看邊和頂點";
        }
        else
        {
            // 實心面 — 用邊加面色
            for (int i = 0; i < tris.Length; i += 3)
            {
                float shade = 0.3f + 0.15f * (i / 3);
                Color c = new Color(shade, shade + 0.1f, shade + 0.2f);
                mr.DrawLine(rv[tris[i]], rv[tris[i + 1]], c, 0.02f);
                mr.DrawLine(rv[tris[i + 1]], rv[tris[i + 2]], c, 0.02f);
                mr.DrawLine(rv[tris[i + 2]], rv[tris[i]], c, 0.02f);

                // 面中心點表示面
                Vector3 center = (rv[tris[i]] + rv[tris[i + 1]] + rv[tris[i + 2]]) / 3f;
                mr.DrawLine(center - Vector3.right * 0.015f, center + Vector3.right * 0.015f, c, 0.03f);
            }
            modeLabel.text = "Solid — 三角面填充";
        }
    }

    public override bool CheckChallengeComplete() => wireframe;
}
