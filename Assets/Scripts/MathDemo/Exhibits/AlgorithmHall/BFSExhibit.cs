using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ============================================================
// BFSExhibit.cs — 廣度優先搜索
// 在共用 15×15 網格上逐層擴展
// ============================================================

public class BFSExhibit : ExhibitBase
{
    private PathfindingGrid grid;
    private TextMesh statusLabel;
    private TextMesh statsLabel;
    private bool isRunning;
    private int explored;
    private int pathLen;

    public override void BuildExhibit()
    {
        exhibitName = "BFS 廣度優先搜索";
        description = "Breadth-First Search\n用佇列（Queue）逐層擴展：\n\n① 起點放入佇列\n② 取出首個節點\n③ 鄰居加入佇列\n④ 重複直到找到終點\n\n🎮 遊戲應用：\n• Flood Fill 漫水填充（油漆桶工具）\n• 塔防遊戲的敵人路徑（無權重最短路）\n• 影響力地圖：從據點擴散的範圍\n• RTS 的霧區探索邊界\n\n按 E 執行 BFS";
        formula = "Time: O(V+E)    Space: O(V)";
        challengeDescription = "完整執行一次 BFS";

        var gridGo = new GameObject("BFSGrid");
        gridGo.transform.SetParent(transform, false);
        gridGo.transform.localPosition = Vector3.zero;
        grid = gridGo.AddComponent<PathfindingGrid>();
        grid.BuildGrid(gridGo.transform);

        statusLabel = CreateLabel(new Vector3(0, -4.5f, 0), "按 E 執行 BFS", 32, Color.white);
        statsLabel = CreateLabel(new Vector3(0, -5.2f, 0), "", 26, new Color(0.7f, 0.8f, 1f));
    }

    public override void UpdateVisualization() { }

    protected override void OnChallengeStart()
    {
        if (isRunning) return;
        isRunning = true;
        grid.ResetPathData();
        grid.UpdateVisuals();
        statusLabel.text = "BFS 執行中...";
        statusLabel.color = new Color(0.3f, 0.7f, 1f);
        StartCoroutine(RunBFS());
    }

    private IEnumerator RunBFS()
    {
        explored = 0;
        var start = grid.StartCell;
        var end = grid.EndCell;

        var queue = new Queue<Vector2Int>();
        queue.Enqueue(start);

        var c = grid.cells[start.x, start.y];
        c.gCost = 0;
        c.visited = true;
        grid.cells[start.x, start.y] = c;

        bool found = false;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            explored++;

            if (current == end) { found = true; break; }

            foreach (var nb in grid.GetNeighbors(current.x, current.y))
            {
                if (grid.cells[nb.x, nb.y].visited) continue;

                var nc = grid.cells[nb.x, nb.y];
                nc.visited = true;
                nc.parentX = current.x;
                nc.parentY = current.y;
                nc.gCost = grid.cells[current.x, current.y].gCost + 1;
                grid.cells[nb.x, nb.y] = nc;

                queue.Enqueue(nb);
            }

            // 動畫：每幾步更新一次
            if (explored % 3 == 0)
            {
                grid.UpdateVisuals();
                yield return new WaitForSeconds(0.05f);
            }
        }

        if (found)
        {
            var path = grid.TracePath(end);
            grid.MarkPath(path);
            pathLen = path.Count;
        }

        grid.UpdateVisuals();
        statusLabel.text = found ? $"✓ 找到！探索 {explored} 格，路徑長 {pathLen}" : "✗ 無路可達";
        statusLabel.color = found ? new Color(0.3f, 1f, 0.5f) : new Color(1f, 0.4f, 0.3f);
        statsLabel.text = $"BFS 特性：無權重最短路、逐層擴展（同心圓）";

        isRunning = false;
        challengeCompleted = true;
        ChallengeSystem.Instance?.MarkCompleted(exhibitName);
    }

    public override bool CheckChallengeComplete() => challengeCompleted;
}
