using UnityEngine;

// ============================================================
// HomogeneousExhibit.cs — 齊次座標
// (x,y,z,w) → w=1 是點、w=0 是方向
// ============================================================

public class HomogeneousExhibit : ExhibitBase
{
    private DragHandle handleW;
    private TextMesh coordLabel, typeLabel, infoLabel;
    private GameObject vizObj;
    private Vector3 basePos = new Vector3(1f, 1f, 0);

    public override void BuildExhibit()
    {
        exhibitName = "齊次座標 Homogeneous";
        description = "齊次座標 (x, y, z, w)：\n\n• w = 1 → 點 (x, y, z)\n• w = 0 → 方向 (x, y, z)\n• 其他 w → 透視除法 (x/w, y/w, z/w)\n\n🎮 遊戲應用：\n• GPU 頂點著色器用 4D 矩陣運算\n• 透視投影：遠處物體自動縮小\n• w=0 的方向向量不受平移影響\n  → 法線、光照方向、速度\n\n拖曳 w 值觀察變化";
        formula = "(x,y,z,w) → (x/w, y/w, z/w)";
        challengeDescription = "讓 w 接近 0 觀察趨於無窮";

        vizObj = CreateStaticPrimitive(PrimitiveType.Sphere, basePos, Vector3.one * 0.2f, new Color(0.3f, 0.9f, 1f));

        handleW = CreateDragHandle(new Vector3(0, -2f, 0), new Color(1f, 0.85f, 0.2f), 0.12f);
        handleW.minBounds = new Vector3(-2.5f, -2f, 0);
        handleW.maxBounds = new Vector3(2.5f, -2f, 0);

        CreateLabel(new Vector3(-2.8f, -2f, 0), "w=0", 22, new Color(0.5f, 0.5f, 0.6f));
        CreateLabel(new Vector3(2.8f, -2f, 0), "w=2", 22, new Color(0.5f, 0.5f, 0.6f));
        CreateLabel(new Vector3(0, -1.3f, 0), "w=1", 18, new Color(0.4f, 0.4f, 0.5f));

        coordLabel = CreateLabel(new Vector3(0, 2.5f, 0), "", 28, Color.white);
        typeLabel = CreateLabel(new Vector3(0, -3f, 0), "", 30, Color.white);
        infoLabel = CreateLabel(new Vector3(0, -3.6f, 0), "", 24, new Color(0.7f, 0.7f, 0.8f));
    }

    public override void UpdateVisualization()
    {
        var mr = MathLineRenderer.Instance;
        if (mr == null) return;

        float t = Mathf.InverseLerp(-2.5f, 2.5f, handleW.LocalPosition.x);
        float w = Mathf.Lerp(0.01f, 2f, t);

        float x = basePos.x;
        float y = basePos.y;

        // 透視除法
        float px = x / w;
        float py = y / w;

        // 限制範圍防止爆掉
        px = Mathf.Clamp(px, -5f, 5f);
        py = Mathf.Clamp(py, -5f, 5f);

        vizObj.transform.localPosition = new Vector3(px, py, 0);

        // 原點到投影點的線
        Vector3 origin = transform.position;
        Vector3 objPos = transform.TransformPoint(new Vector3(px, py, 0));
        mr.DrawLine(origin, objPos, new Color(0.3f, 0.5f, 0.8f, 0.5f), 0.008f);

        coordLabel.text = $"({x:F1}, {y:F1}, 0, {w:F2}) → ({px:F2}, {py:F2}, 0)";

        if (w > 0.95f && w < 1.05f)
        {
            typeLabel.text = "w ≈ 1 → 這是一個「點」";
            typeLabel.color = new Color(0.3f, 0.9f, 0.5f);
            infoLabel.text = "座標不變，正常的 3D 位置";
        }
        else if (w < 0.1f)
        {
            typeLabel.text = "w → 0 → 這是一個「方向」";
            typeLabel.color = new Color(1f, 0.4f, 0.3f);
            infoLabel.text = "無窮遠！平移對方向無效";
        }
        else
        {
            typeLabel.text = $"w = {w:F2} → 透視除法";
            typeLabel.color = new Color(1f, 0.85f, 0.3f);
            infoLabel.text = "w ≠ 1 時座標被縮放 — GPU 投影用此原理";
        }
    }

    public override bool CheckChallengeComplete()
    {
        float t = Mathf.InverseLerp(-2.5f, 2.5f, handleW.LocalPosition.x);
        float w = Mathf.Lerp(0.01f, 2f, t);
        return w < 0.15f;
    }
}
