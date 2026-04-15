using UnityEngine;
using System.Collections;

// ============================================================
// MergeSortExhibit.cs — 合併排序
// 16 根 3D 柱子的分治 + 合併動畫
// ============================================================

public class MergeSortExhibit : ExhibitBase
{
    private SortVisualizer sortVis;
    private TextMesh statusLabel;
    private TextMesh compLabel;
    private bool isRunning;

    public override void BuildExhibit()
    {
        exhibitName = "合併排序 Merge Sort";
        description = "分治法 (Divide & Conquer) 經典：\n\n① 把陣列切成兩半（Divide）\n② 各半遞迴排序（Conquer）\n③ 兩個已排好的合併（Merge）\n\n🎮 遊戲應用：\n• 渲染排序（透明物件由遠到近）\n• 排行榜更新\n• 穩定排序：相同分數保持原順序\n• O(n log n) 永遠穩定高效\n\n按 E 開始排序動畫";
        formula = "T(n) = 2T(n/2) + O(n) → O(n log n)";
        challengeDescription = "完整觀看一次排序過程";

        var visGo = new GameObject("SortVis");
        visGo.transform.SetParent(transform, false);
        visGo.transform.localPosition = Vector3.up * 0.5f;
        sortVis = visGo.AddComponent<SortVisualizer>();
        sortVis.count = 16;
        sortVis.Build(visGo.transform);

        statusLabel = CreateLabel(new Vector3(0, -1.5f, 0), "按 E 開始排序", 34, Color.white);
        compLabel = CreateLabel(new Vector3(0, -2.2f, 0), "O(n log n) = O(16 × 4) = ~64 次比較", 26, new Color(0.7f, 0.7f, 0.8f));
    }

    public override void UpdateVisualization() { }

    protected override void OnChallengeStart()
    {
        if (isRunning) return;
        isRunning = true;
        sortVis.Shuffle();
        statusLabel.text = "排序中...";
        statusLabel.color = new Color(1f, 0.85f, 0.3f);
        StartCoroutine(RunSort());
    }

    private IEnumerator RunSort()
    {
        yield return sortVis.MergeSortCoroutine();
        statusLabel.text = "✓ 排序完成！按 E 再跑一次";
        statusLabel.color = new Color(0.3f, 1f, 0.5f);
        isRunning = false;
        challengeCompleted = true;
        ChallengeSystem.Instance?.MarkCompleted(exhibitName);
    }

    public override bool CheckChallengeComplete() => challengeCompleted;
}
