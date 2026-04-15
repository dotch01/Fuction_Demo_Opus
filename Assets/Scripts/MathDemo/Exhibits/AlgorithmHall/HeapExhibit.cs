using UnityEngine;
using System.Collections;

// ============================================================
// HeapExhibit.cs — Heap 堆
// 二元樹可視化 + 插入/取出動畫
// ============================================================

public class HeapExhibit : ExhibitBase
{
    private TreeVisualizer treeVis;
    private TextMesh statusLabel;
    private TextMesh infoLabel;
    private TextMesh arrayLabel;
    private int nextInsert = 1;
    private bool isAnimating;

    public override void BuildExhibit()
    {
        exhibitName = "堆 Heap";
        description = "一種特殊的完全二元樹：\n\n• Min Heap：父 ≤ 子（根 = 最小值）\n• Max Heap：父 ≥ 子（根 = 最大值）\n\n插入（Sift Up）：往上冒泡\n取出（Sift Down）：往下沉\n\n🎮 遊戲應用：\n• A* 尋路的 Open List（Priority Queue）\n• 事件排程系統（下一個觸發事件）\n• 技能冷卻管理（最快解鎖的技能）\n• O(log n) 插入/取出\n\n按 E 插入，再按 E 取出";
        formula = "Insert / Extract: O(log n)    Build: O(n)";
        challengeDescription = "插入 5 個值再取出根";

        var treeGo = new GameObject("TreeVis");
        treeGo.transform.SetParent(transform, false);
        treeGo.transform.localPosition = Vector3.zero;
        treeVis = treeGo.AddComponent<TreeVisualizer>();
        treeVis.Build(treeGo.transform);

        statusLabel = CreateLabel(new Vector3(0, -2f, 0), "按 E 插入值", 32, Color.white);
        infoLabel = CreateLabel(new Vector3(0, -2.7f, 0), "Min Heap — 根 = 最小值", 26, new Color(0.7f, 0.8f, 1f));
        arrayLabel = CreateLabel(new Vector3(0, -3.3f, 0), "陣列：[]", 24, new Color(0.6f, 0.6f, 0.7f));
    }

    public override void UpdateVisualization()
    {
        if (MathLineRenderer.Instance == null) return;
        treeVis.DrawEdges(MathLineRenderer.Instance, treeVis.transform);

        // 陣列表示
        string arr = "[" + string.Join(", ", treeVis.HeapValues) + "]";
        arrayLabel.text = $"陣列：{arr}";
    }

    protected override void OnChallengeStart()
    {
        if (isAnimating) return;
        isAnimating = true;

        if (treeVis.HeapValues.Count < 7)
        {
            // 插入
            int val = Random.Range(1, 50);
            statusLabel.text = $"插入 {val}...";
            statusLabel.color = new Color(0.3f, 0.9f, 0.5f);
            StartCoroutine(DoInsert(val));
        }
        else
        {
            // 取出
            statusLabel.text = "取出根節點...";
            statusLabel.color = new Color(1f, 0.5f, 0.3f);
            StartCoroutine(DoExtract());
        }
    }

    private IEnumerator DoInsert(int val)
    {
        yield return treeVis.InsertAnimated(val);
        int root = treeVis.HeapValues.Count > 0 ? treeVis.HeapValues[0] : 0;
        statusLabel.text = $"✓ 已插入 {val}（根 = {root}）按 E 繼續";
        statusLabel.color = Color.white;
        isAnimating = false;

        if (treeVis.HeapValues.Count >= 5 && !challengeCompleted)
        {
            infoLabel.text = "已插入 5+ 個！再按 E 取出根試試";
        }
    }

    private IEnumerator DoExtract()
    {
        yield return treeVis.ExtractRootAnimated();
        int root = treeVis.HeapValues.Count > 0 ? treeVis.HeapValues[0] : 0;
        statusLabel.text = $"✓ 已取出！新根 = {root}　按 E 繼續";
        statusLabel.color = Color.white;
        isAnimating = false;

        if (!challengeCompleted)
        {
            challengeCompleted = true;
            ChallengeSystem.Instance?.MarkCompleted(exhibitName);
        }
    }

    public override bool CheckChallengeComplete() => challengeCompleted;
}
