using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ============================================================
// DijkstraExhibit.cs — Dijkstra 最短路
// 帶權重的網格搜索
// ============================================================

public class DijkstraExhibit : ExhibitBase
{
    private PathfindingGrid grid;
    private TextMesh statusLabel;
    private TextMesh statsLabel;
    private bool isRunning;
    private int explored;
    private int totalCost;

    public override void BuildExhibit()
    {
        exhibitName = "Dijkstra 演算法";
        description = "帶權重的最短路：\n\n每格有不同移動成本：\n  綠 = 1（平地）\n  黃 = 2（沼澤）\n  棕 = 3（山地）\n\n🎮 遊戲應用：\n• NavMesh 導航中考慮地形成本\n• RTS 部隊繞開困難地形\n• 網路遊戲的路由優化\n• RPG 旅行系統的最佳路線\n\n對比 BFS（只看步數）\n按 E 執行";
        formula = "Time: O((V+E) log V)    f(n) = g(n)";
        challengeDescription = "完整執行一次 Dijkstra";

        var gridGo = new GameObject("DijkstraGrid");
        gridGo.transform.SetParent(transform, false);
        gridGo.transform.localPosition = Vector3.zero;
        grid = gridGo.AddComponent<PathfindingGrid>();
        grid.BuildGrid(gridGo.transform);

        statusLabel = CreateLabel(new Vector3(0, -4.5f, 0), "按 E 執行 Dijkstra", 32, Color.white);
        statsLabel = CreateLabel(new Vector3(0, -5.2f, 0), "有權重地圖（黃=2倍、棕=3倍成本）", 26, new Color(0.7f, 0.8f, 1f));
    }

    public override void UpdateVisualization() { }

    protected override void OnChallengeStart()
    {
        if (isRunning) return;
        isRunning = true;
        grid.ResetPathData();
        grid.UpdateVisuals();
        statusLabel.text = "Dijkstra 執行中...";
        statusLabel.color = new Color(0.3f, 0.8f, 0.5f);
        StartCoroutine(RunDijkstra());
    }

    private IEnumerator RunDijkstra()
    {
        explored = 0;
        var start = grid.StartCell;
        var end = grid.EndCell;

        // 簡易優先佇列（用 SortedSet 模擬）
        var open = new SortedList<int, List<Vector2Int>>();

        var sc = grid.cells[start.x, start.y];
        sc.gCost = 0;
        grid.cells[start.x, start.y] = sc;
        AddToOpen(open, 0, start);

        bool found = false;

        while (open.Count > 0)
        {
            // 取最小 gCost
            var firstKey = open.Keys[0];
            var list = open[firstKey];
            var current = list[0];
            list.RemoveAt(0);
            if (list.Count == 0) open.Remove(firstKey);

            if (grid.cells[current.x, current.y].inClosed) continue;

            var cc = grid.cells[current.x, current.y];
            cc.inClosed = true;
            cc.visited = true;
            grid.cells[current.x, current.y] = cc;
            explored++;

            if (current == end) { found = true; break; }

            foreach (var nb in grid.GetNeighbors(current.x, current.y))
            {
                if (grid.cells[nb.x, nb.y].inClosed) continue;

                int newG = grid.cells[current.x, current.y].gCost + grid.cells[nb.x, nb.y].weight;
                if (newG < grid.cells[nb.x, nb.y].gCost)
                {
                    var nc = grid.cells[nb.x, nb.y];
                    nc.gCost = newG;
                    nc.parentX = current.x;
                    nc.parentY = current.y;
                    nc.inOpen = true;
                    grid.cells[nb.x, nb.y] = nc;
                    AddToOpen(open, newG, nb);
                }
            }

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
            totalCost = grid.cells[end.x, end.y].gCost;
        }

        grid.UpdateVisuals();
        statusLabel.text = found ? $"✓ 找到！探索 {explored} 格，總成本 {totalCost}" : "✗ 無路可達";
        statusLabel.color = found ? new Color(0.3f, 1f, 0.5f) : new Color(1f, 0.4f, 0.3f);
        statsLabel.text = "Dijkstra：考慮權重，找最小總成本路徑";

        isRunning = false;
        challengeCompleted = true;
        ChallengeSystem.Instance?.MarkCompleted(exhibitName);
    }

    private void AddToOpen(SortedList<int, List<Vector2Int>> open, int key, Vector2Int pos)
    {
        if (!open.ContainsKey(key)) open[key] = new List<Vector2Int>();
        open[key].Add(pos);
    }

    public override bool CheckChallengeComplete() => challengeCompleted;
}
