using System.Collections.Generic;
using UnityEngine;

// ============================================================
// MathLineRenderer.cs
// 數學繪圖核心 — 提供向量箭頭、線段、圓、弧等即時繪製
// 使用 LineRenderer 組件池，每幀重用
// ============================================================

public class MathLineRenderer : MonoBehaviour
{
    public static MathLineRenderer Instance { get; private set; }

    private readonly List<LineRenderer> linePool = new List<LineRenderer>();
    private int activeCount = 0;
    private Material lineMaterial;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        var shader = Shader.Find("Universal Render Pipeline/Unlit");
        lineMaterial = new Material(shader);
    }

    void LateUpdate()
    {
        // 隱藏未使用的
        for (int i = activeCount; i < linePool.Count; i++)
        {
            if (linePool[i].gameObject.activeSelf)
                linePool[i].gameObject.SetActive(false);
        }
        activeCount = 0;
    }

    // --------------------------------------------------------
    // 公開繪圖 API
    // --------------------------------------------------------

    /// <summary>畫一條線段</summary>
    public void DrawLine(Vector3 from, Vector3 to, Color color, float width = 0.02f)
    {
        var lr = GetLine();
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.positionCount = 2;
        lr.SetPosition(0, from);
        lr.SetPosition(1, to);
    }

    /// <summary>畫一個向量箭頭（線段 + 箭頭三角形）</summary>
    public void DrawArrow(Vector3 from, Vector3 to, Color color, float width = 0.03f, float headLength = 0.15f)
    {
        Vector3 dir = (to - from).normalized;
        float length = Vector3.Distance(from, to);
        if (length < 0.001f) return;

        float actualHead = Mathf.Min(headLength, length * 0.3f);
        Vector3 bodyEnd = to - dir * actualHead;

        // 主線段
        DrawLine(from, bodyEnd, color, width);

        // 箭頭（用較寬的線段模擬）
        Vector3 right = Vector3.Cross(dir, Vector3.forward).normalized;
        if (right.sqrMagnitude < 0.001f) right = Vector3.Cross(dir, Vector3.up).normalized;

        Vector3 arrowBase1 = bodyEnd + right * actualHead * 0.4f;
        Vector3 arrowBase2 = bodyEnd - right * actualHead * 0.4f;

        DrawLine(arrowBase1, to, color, width * 0.5f);
        DrawLine(arrowBase2, to, color, width * 0.5f);
        DrawLine(arrowBase1, arrowBase2, color, width * 0.5f);
    }

    /// <summary>畫一個圓</summary>
    public void DrawCircle(Vector3 center, float radius, Vector3 normal, Color color, float width = 0.015f, int segments = 48)
    {
        var lr = GetLine();
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.positionCount = segments + 1;
        lr.loop = false;

        Quaternion rot = Quaternion.LookRotation(normal);
        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            Vector3 localPos = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            lr.SetPosition(i, center + rot * localPos);
        }
    }

    /// <summary>畫一段弧線（用於顯示角度）</summary>
    public void DrawArc(Vector3 center, Vector3 fromDir, float angle, float radius, Color color, float width = 0.015f, int segments = 24)
    {
        var lr = GetLine();
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.positionCount = segments + 1;
        lr.loop = false;

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments * angle;
            Vector3 dir = Quaternion.AngleAxis(t, Vector3.forward) * fromDir;
            lr.SetPosition(i, center + dir * radius);
        }
    }

    /// <summary>畫虛線</summary>
    public void DrawDashedLine(Vector3 from, Vector3 to, Color color, float width = 0.015f, float dashLength = 0.1f)
    {
        float dist = Vector3.Distance(from, to);
        Vector3 dir = (to - from).normalized;
        int dashes = Mathf.Max(1, Mathf.FloorToInt(dist / (dashLength * 2f)));

        for (int i = 0; i < dashes; i++)
        {
            float t0 = (float)i / dashes;
            float t1 = t0 + 0.5f / dashes;
            DrawLine(Vector3.Lerp(from, to, t0), Vector3.Lerp(from, to, t1), color, width);
        }
    }

    /// <summary>畫一個點（小十字）</summary>
    public void DrawPoint(Vector3 pos, Color color, float size = 0.08f)
    {
        DrawLine(pos - Vector3.right * size, pos + Vector3.right * size, color, 0.025f);
        DrawLine(pos - Vector3.up * size, pos + Vector3.up * size, color, 0.025f);
    }

    /// <summary>畫填充三角形（半透明）</summary>
    public void DrawTriangle(Vector3 a, Vector3 b, Vector3 c, Color color, float width = 0.015f)
    {
        DrawLine(a, b, color, width);
        DrawLine(b, c, color, width);
        DrawLine(c, a, color, width);
    }

    /// <summary>畫一個 AABB 方框</summary>
    public void DrawAABB(Vector3 min, Vector3 max, Color color, float width = 0.015f)
    {
        // 底面
        DrawLine(new Vector3(min.x, min.y, min.z), new Vector3(max.x, min.y, min.z), color, width);
        DrawLine(new Vector3(max.x, min.y, min.z), new Vector3(max.x, min.y, max.z), color, width);
        DrawLine(new Vector3(max.x, min.y, max.z), new Vector3(min.x, min.y, max.z), color, width);
        DrawLine(new Vector3(min.x, min.y, max.z), new Vector3(min.x, min.y, min.z), color, width);
        // 頂面
        DrawLine(new Vector3(min.x, max.y, min.z), new Vector3(max.x, max.y, min.z), color, width);
        DrawLine(new Vector3(max.x, max.y, min.z), new Vector3(max.x, max.y, max.z), color, width);
        DrawLine(new Vector3(max.x, max.y, max.z), new Vector3(min.x, max.y, max.z), color, width);
        DrawLine(new Vector3(min.x, max.y, max.z), new Vector3(min.x, max.y, min.z), color, width);
        // 柱子
        DrawLine(new Vector3(min.x, min.y, min.z), new Vector3(min.x, max.y, min.z), color, width);
        DrawLine(new Vector3(max.x, min.y, min.z), new Vector3(max.x, max.y, min.z), color, width);
        DrawLine(new Vector3(max.x, min.y, max.z), new Vector3(max.x, max.y, max.z), color, width);
        DrawLine(new Vector3(min.x, min.y, max.z), new Vector3(min.x, max.y, max.z), color, width);
    }

    /// <summary>在 3D 位置顯示數值文字（需外部 TextMesh）</summary>
    public void UpdateValueLabel(TextMesh tm, string text)
    {
        if (tm != null) tm.text = text;
    }

    // --------------------------------------------------------
    // 線段池管理
    // --------------------------------------------------------

    private LineRenderer GetLine()
    {
        LineRenderer lr;
        if (activeCount < linePool.Count)
        {
            lr = linePool[activeCount];
            lr.gameObject.SetActive(true);
            lr.loop = false;
        }
        else
        {
            var go = new GameObject($"Line_{linePool.Count}");
            go.transform.SetParent(transform, false);
            lr = go.AddComponent<LineRenderer>();
            lr.material = new Material(lineMaterial);
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.allowOcclusionWhenDynamic = false;
            lr.useWorldSpace = true;
            linePool.Add(lr);
        }
        activeCount++;
        return lr;
    }
}
