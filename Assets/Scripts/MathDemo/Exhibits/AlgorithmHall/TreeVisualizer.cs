using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ============================================================
// TreeVisualizer.cs — 二元樹/堆 視覺化共用元件
// 球體節點 + 連線 + sift 動畫
// ============================================================

public class TreeVisualizer : MonoBehaviour
{
    public int maxNodes = 31; // 5 層完全二元樹
    public float nodeRadius = 0.15f;
    public float levelGap = 0.7f;
    public float baseWidth = 5f;

    private List<int> heapValues = new List<int>();
    private GameObject[] nodeObjects;
    private Renderer[] nodeRenderers;
    private TextMesh[] nodeTexts;
    private bool isMinHeap = true;
    private float animSpeed = 1f;

    private static readonly Color normalColor    = new Color(0.3f, 0.55f, 0.85f);
    private static readonly Color highlightColor = new Color(1f, 0.85f, 0.2f);
    private static readonly Color swapColor      = new Color(1f, 0.35f, 0.3f);
    private static readonly Color insertedColor  = new Color(0.3f, 0.9f, 0.5f);

    public List<int> HeapValues => heapValues;
    public bool IsMinHeap { get => isMinHeap; set => isMinHeap = value; }
    public float AnimSpeed { get => animSpeed; set => animSpeed = Mathf.Max(0.1f, value); }

    public void Build(Transform parent)
    {
        nodeObjects = new GameObject[maxNodes];
        nodeRenderers = new Renderer[maxNodes];
        nodeTexts = new TextMesh[maxNodes];

        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));

        for (int i = 0; i < maxNodes; i++)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = $"Node_{i}";
            go.transform.SetParent(parent, false);
            go.transform.localScale = Vector3.one * nodeRadius * 2f;
            go.SetActive(false);

            var r = go.GetComponent<Renderer>();
            r.material = new Material(mat);
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);

            // 文字
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            textGo.transform.localPosition = Vector3.zero;
            textGo.transform.localScale = Vector3.one * 0.3f;
            var tm = textGo.AddComponent<TextMesh>();
            tm.fontSize = 40;
            tm.characterSize = 0.08f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = Color.white;

            nodeObjects[i] = go;
            nodeRenderers[i] = r;
            nodeTexts[i] = tm;
        }
    }

    public Vector3 GetNodePosition(int index)
    {
        int level = 0;
        int temp = index + 1;
        while (temp > 1) { temp >>= 1; level++; }

        int nodesInLevel = 1 << level;
        int indexInLevel = index - (nodesInLevel - 1);
        float width = baseWidth / Mathf.Pow(1.3f, level);
        float x = Mathf.Lerp(-width, width, (indexInLevel + 0.5f) / nodesInLevel);
        float y = 2f - level * levelGap;

        return new Vector3(x, y, 0);
    }

    public void UpdateVisuals()
    {
        for (int i = 0; i < maxNodes; i++)
        {
            if (i < heapValues.Count)
            {
                nodeObjects[i].SetActive(true);
                nodeObjects[i].transform.localPosition = GetNodePosition(i);
                nodeTexts[i].text = heapValues[i].ToString();
                nodeRenderers[i].material.color = normalColor;
            }
            else
            {
                nodeObjects[i].SetActive(false);
            }
        }
    }

    public void DrawEdges(MathLineRenderer mr, Transform parent)
    {
        for (int i = 0; i < heapValues.Count; i++)
        {
            int left = 2 * i + 1;
            int right = 2 * i + 2;

            Vector3 pos = parent.TransformPoint(GetNodePosition(i));
            if (left < heapValues.Count)
            {
                Vector3 lp = parent.TransformPoint(GetNodePosition(left));
                mr.DrawLine(pos, lp, new Color(0.5f, 0.5f, 0.6f), 0.01f);
            }
            if (right < heapValues.Count)
            {
                Vector3 rp = parent.TransformPoint(GetNodePosition(right));
                mr.DrawLine(pos, rp, new Color(0.5f, 0.5f, 0.6f), 0.01f);
            }
        }
    }

    // --------------------------------------------------------
    // Heap 操作 + 動畫
    // --------------------------------------------------------

    public IEnumerator InsertAnimated(int value)
    {
        heapValues.Add(value);
        int idx = heapValues.Count - 1;
        UpdateVisuals();
        nodeRenderers[idx].material.color = insertedColor;
        yield return new WaitForSeconds(0.3f / animSpeed);

        // Sift up
        while (idx > 0)
        {
            int parent = (idx - 1) / 2;
            bool shouldSwap = isMinHeap ? heapValues[idx] < heapValues[parent]
                                        : heapValues[idx] > heapValues[parent];

            nodeRenderers[idx].material.color = highlightColor;
            nodeRenderers[parent].material.color = highlightColor;
            yield return new WaitForSeconds(0.3f / animSpeed);

            if (shouldSwap)
            {
                nodeRenderers[idx].material.color = swapColor;
                nodeRenderers[parent].material.color = swapColor;
                (heapValues[idx], heapValues[parent]) = (heapValues[parent], heapValues[idx]);
                UpdateVisuals();
                yield return new WaitForSeconds(0.25f / animSpeed);
                idx = parent;
            }
            else
            {
                nodeRenderers[idx].material.color = normalColor;
                nodeRenderers[parent].material.color = normalColor;
                break;
            }
        }
        UpdateVisuals();
    }

    public IEnumerator ExtractRootAnimated()
    {
        if (heapValues.Count == 0) yield break;

        nodeRenderers[0].material.color = swapColor;
        yield return new WaitForSeconds(0.3f / animSpeed);

        int last = heapValues.Count - 1;
        heapValues[0] = heapValues[last];
        heapValues.RemoveAt(last);

        if (heapValues.Count == 0) { UpdateVisuals(); yield break; }

        UpdateVisuals();
        yield return new WaitForSeconds(0.2f / animSpeed);

        // Sift down
        int idx = 0;
        while (true)
        {
            int left = 2 * idx + 1;
            int right = 2 * idx + 2;
            int target = idx;

            if (left < heapValues.Count)
            {
                bool cmp = isMinHeap ? heapValues[left] < heapValues[target]
                                     : heapValues[left] > heapValues[target];
                if (cmp) target = left;
            }
            if (right < heapValues.Count)
            {
                bool cmp = isMinHeap ? heapValues[right] < heapValues[target]
                                     : heapValues[right] > heapValues[target];
                if (cmp) target = right;
            }

            if (target == idx) break;

            nodeRenderers[idx].material.color = highlightColor;
            nodeRenderers[target].material.color = highlightColor;
            yield return new WaitForSeconds(0.3f / animSpeed);

            nodeRenderers[idx].material.color = swapColor;
            nodeRenderers[target].material.color = swapColor;
            (heapValues[idx], heapValues[target]) = (heapValues[target], heapValues[idx]);
            UpdateVisuals();
            yield return new WaitForSeconds(0.25f / animSpeed);

            idx = target;
        }
        UpdateVisuals();
    }

    public void Reset()
    {
        heapValues.Clear();
        UpdateVisuals();
    }
}
