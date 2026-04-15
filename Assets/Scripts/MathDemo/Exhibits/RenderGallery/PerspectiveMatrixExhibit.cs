using UnityEngine;

// ============================================================
// PerspectiveMatrixExhibit.cs — 透視矩陣
// FOV / Near / Far 控制
// ============================================================

public class PerspectiveMatrixExhibit : ExhibitBase
{
    private DragHandle handleFOV, handleNear;
    private TextMesh fovLabel, nearLabel, matLabel;

    public override void BuildExhibit()
    {
        exhibitName = "透視矩陣 Perspective";
        description = "透視投影讓遠的物體看起來小：\n\n參數：\n• FOV = 視野角度（垂直）\n• Near/Far = 近/遠裁切面\n• Aspect = 寬高比\n\n🎮 遊戲應用：\n• FPS：FOV 60-90°（太小暈、太大變形）\n• 狙擊鏡：縮小 FOV = 放大效果\n• VR：需要精確的透視設定避免暈眩\n• Camera.fieldOfView 就是這個 FOV\n\n拖曳控制 FOV 和 Near";
        formula = "P[0][0] = 1/(aspect·tan(fov/2))\nP[1][1] = 1/tan(fov/2)";
        challengeDescription = "讓 FOV > 90°（超廣角）";

        handleFOV = CreateDragHandle(new Vector3(-2.5f, 0, 0), new Color(0.3f, 0.8f, 1f), 0.12f);
        handleFOV.minBounds = new Vector3(-2.5f, -1.5f, 0);
        handleFOV.maxBounds = new Vector3(-2.5f, 1.5f, 0);

        handleNear = CreateDragHandle(new Vector3(2.5f, 0, 0), new Color(1f, 0.5f, 0.3f), 0.12f);
        handleNear.minBounds = new Vector3(2.5f, -1.5f, 0);
        handleNear.maxBounds = new Vector3(2.5f, 1.5f, 0);

        CreateLabel(new Vector3(-2.5f, -2f, 0), "FOV", 20, new Color(0.5f, 0.5f, 0.6f));
        CreateLabel(new Vector3(2.5f, -2f, 0), "Near", 20, new Color(0.5f, 0.5f, 0.6f));

        fovLabel = CreateLabel(new Vector3(0, -2.5f, 0), "", 28, Color.white);
        nearLabel = CreateLabel(new Vector3(0, -3.1f, 0), "", 26, new Color(0.7f, 0.7f, 0.8f));
        matLabel = CreateLabel(new Vector3(0, -3.7f, 0), "", 22, new Color(0.6f, 0.6f, 0.7f));
    }

    public override void UpdateVisualization()
    {
        var mr = MathLineRenderer.Instance;
        if (mr == null) return;

        float fov = Mathf.Lerp(20f, 120f, Mathf.InverseLerp(-1.5f, 1.5f, handleFOV.LocalPosition.y));
        float near = Mathf.Lerp(0.1f, 2f, Mathf.InverseLerp(-1.5f, 1.5f, handleNear.LocalPosition.y));
        float far = 5f;
        float aspect = 1.6f;

        float halfH_near = near * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
        float halfW_near = halfH_near * aspect;
        float halfH_far = far * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
        float halfW_far = halfH_far * aspect;

        // 畫 frustum（俯視 XZ 平面投影，scale down）
        float s = 0.3f;
        Vector3 eye = transform.TransformPoint(new Vector3(0, 0, 0));

        Vector3 nl = transform.TransformPoint(new Vector3(-halfW_near * s, 0, near * s));
        Vector3 nr = transform.TransformPoint(new Vector3(halfW_near * s, 0, near * s));
        Vector3 fl = transform.TransformPoint(new Vector3(-halfW_far * s, 0, far * s));
        Vector3 fr = transform.TransformPoint(new Vector3(halfW_far * s, 0, far * s));

        // Frustum lines
        mr.DrawLine(eye, fl, new Color(0.3f, 0.8f, 1f, 0.5f), 0.006f);
        mr.DrawLine(eye, fr, new Color(0.3f, 0.8f, 1f, 0.5f), 0.006f);
        mr.DrawLine(nl, nr, new Color(0.3f, 1f, 0.5f), 0.01f); // near plane
        mr.DrawLine(fl, fr, new Color(1f, 0.5f, 0.3f), 0.01f); // far plane

        // 物體在不同距離的投影大小
        for (float d = near + 0.5f; d < far; d += 1f)
        {
            float hh = d * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
            float w = hh * aspect * s;
            Vector3 l = transform.TransformPoint(new Vector3(-w, 0, d * s));
            Vector3 r = transform.TransformPoint(new Vector3(w, 0, d * s));
            mr.DrawLine(l, r, new Color(0.5f, 0.5f, 0.6f, 0.3f), 0.005f);
        }

        float p00 = 1f / (aspect * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad));
        float p11 = 1f / Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);

        fovLabel.text = $"FOV = {fov:F0}°  {(fov > 90 ? "超廣角！" : fov < 40 ? "望遠" : "標準")}";
        nearLabel.text = $"Near = {near:F2}    Far = {far:F1}";
        matLabel.text = $"P[0][0]={p00:F3}  P[1][1]={p11:F3}";
    }

    public override bool CheckChallengeComplete()
    {
        float fov = Mathf.Lerp(20f, 120f, Mathf.InverseLerp(-1.5f, 1.5f, handleFOV.LocalPosition.y));
        return fov > 90f;
    }
}
