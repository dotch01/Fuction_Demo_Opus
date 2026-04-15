using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ============================================================
// SortVisualizer.cs — 3D 方塊柱狀圖排序動畫共用元件
// 合併排序 / 二分搜索展品共用
// ============================================================

public class SortVisualizer : MonoBehaviour
{
    public int count = 16;
    public float barWidth = 0.3f;
    public float maxHeight = 3f;
    public float spacing = 0.35f;

    private int[] values;
    private GameObject[] bars;
    private Renderer[] barRenderers;
    private float animSpeed = 1f;

    private static readonly Color defaultColor = new Color(0.3f, 0.5f, 0.8f);
    private static readonly Color compareColor = new Color(1f, 0.9f, 0.2f);
    private static readonly Color swapColor    = new Color(1f, 0.3f, 0.3f);
    private static readonly Color sortedColor  = new Color(0.2f, 0.9f, 0.4f);
    private static readonly Color searchColor  = new Color(0.4f, 0.4f, 0.9f);
    private static readonly Color midColor     = new Color(1f, 0.85f, 0.2f);
    private static readonly Color foundColor   = new Color(0.2f, 1f, 0.5f);
    private static readonly Color excludeColor = new Color(0.25f, 0.25f, 0.3f);

    public int[] Values => values;
    public float AnimSpeed { get => animSpeed; set => animSpeed = Mathf.Max(0.1f, value); }

    public void Build(Transform parent)
    {
        values = new int[count];
        bars = new GameObject[count];
        barRenderers = new Renderer[count];

        float totalW = count * spacing;
        float startX = -totalW / 2f;

        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));

        for (int i = 0; i < count; i++)
        {
            values[i] = i + 1; // 1..count
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"Bar_{i}";
            go.transform.SetParent(parent, false);

            var r = go.GetComponent<Renderer>();
            r.material = new Material(mat);
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);

            bars[i] = go;
            barRenderers[i] = r;
        }

        Shuffle();
        UpdateVisuals();
    }

    public void Shuffle()
    {
        // Fisher-Yates
        for (int i = count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (values[i], values[j]) = (values[j], values[i]);
        }
        ResetColors();
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        float totalW = count * spacing;
        float startX = -totalW / 2f;

        for (int i = 0; i < count; i++)
        {
            float h = ((float)values[i] / count) * maxHeight;
            bars[i].transform.localPosition = new Vector3(startX + i * spacing, h / 2f, 0);
            bars[i].transform.localScale = new Vector3(barWidth, h, barWidth);
        }
    }

    // --------------------------------------------------------
    // 顏色控制
    // --------------------------------------------------------

    public void SetColor(int index, Color c) => barRenderers[index].material.color = c;
    public void ResetColors() { for (int i = 0; i < count; i++) SetColor(i, defaultColor); }
    public void SetAllSorted() { for (int i = 0; i < count; i++) SetColor(i, sortedColor); }

    // --------------------------------------------------------
    // 合併排序 Coroutine
    // --------------------------------------------------------

    public IEnumerator MergeSortCoroutine()
    {
        ResetColors();
        yield return MergeSortRecursive(0, count - 1);
        SetAllSorted();
        UpdateVisuals();
    }

    private IEnumerator MergeSortRecursive(int left, int right)
    {
        if (left >= right) yield break;
        int mid = (left + right) / 2;

        // 標示分組
        for (int i = left; i <= mid; i++) SetColor(i, new Color(0.5f, 0.3f, 0.7f));
        for (int i = mid + 1; i <= right; i++) SetColor(i, new Color(0.3f, 0.5f, 0.7f));
        yield return new WaitForSeconds(0.3f / animSpeed);

        yield return MergeSortRecursive(left, mid);
        yield return MergeSortRecursive(mid + 1, right);
        yield return Merge(left, mid, right);
    }

    private IEnumerator Merge(int left, int mid, int right)
    {
        int[] temp = new int[right - left + 1];
        int i = left, j = mid + 1, k = 0;

        while (i <= mid && j <= right)
        {
            SetColor(i, compareColor);
            SetColor(j, compareColor);
            yield return new WaitForSeconds(0.15f / animSpeed);

            if (values[i] <= values[j])
            {
                temp[k++] = values[i];
                SetColor(i, sortedColor);
                i++;
            }
            else
            {
                temp[k++] = values[j];
                SetColor(j, swapColor);
                j++;
            }
            yield return new WaitForSeconds(0.1f / animSpeed);
        }

        while (i <= mid) { temp[k++] = values[i]; SetColor(i, sortedColor); i++; }
        while (j <= right) { temp[k++] = values[j]; SetColor(j, sortedColor); j++; }

        for (int t = 0; t < temp.Length; t++)
        {
            values[left + t] = temp[t];
            SetColor(left + t, sortedColor);
        }

        UpdateVisuals();
        yield return new WaitForSeconds(0.2f / animSpeed);
    }

    // --------------------------------------------------------
    // 二分搜索 Coroutine
    // --------------------------------------------------------

    public IEnumerator BinarySearchCoroutine(int target)
    {
        // 先排序（已排好 = 直接用已排列）
        System.Array.Sort(values);
        UpdateVisuals();
        ResetColors();

        // 搜索範圍全亮
        for (int i = 0; i < count; i++) SetColor(i, searchColor);
        yield return new WaitForSeconds(0.5f / animSpeed);

        int lo = 0, hi = count - 1;
        int comparisons = 0;
        bool found = false;

        while (lo <= hi)
        {
            int mid = (lo + hi) / 2;
            comparisons++;

            // 顯示 lo/hi/mid
            for (int i = 0; i < count; i++)
            {
                if (i < lo || i > hi) SetColor(i, excludeColor);
                else SetColor(i, searchColor);
            }
            SetColor(mid, midColor);
            yield return new WaitForSeconds(0.6f / animSpeed);

            if (values[mid] == target)
            {
                SetColor(mid, foundColor);
                found = true;
                break;
            }
            else if (values[mid] < target)
            {
                for (int i = lo; i <= mid; i++) SetColor(i, excludeColor);
                lo = mid + 1;
            }
            else
            {
                for (int i = mid; i <= hi; i++) SetColor(i, excludeColor);
                hi = mid - 1;
            }
            yield return new WaitForSeconds(0.3f / animSpeed);
        }

        if (!found)
        {
            // 沒找到 — 全紅閃一下
            for (int i = 0; i < count; i++) SetColor(i, swapColor);
        }
    }
}
