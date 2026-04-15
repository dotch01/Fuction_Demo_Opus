using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ============================================================
// AStarExhibit.cs — A* 尋路
// 啟發式搜索，大幅減少探索量
// ============================================================

public class AStarExhibit : ExhibitBase
{
    private PathfindingGrid grid;
    private TextMesh statusLabel;
    private TextMesh statsLabel;
    private bool isRunning;
    private int explored;
    private int totalCost;

    public override void BuildExhibit()
    {
        exhibitName = "A* 尋路 A-Star";
        description = "A* = Dijkstra + 啟發式！\n\nf(n) = g(n) + h(n)\n• g(n) = 起點到 n 的實際成本\n• h(n) = n 到終點的估計\n\n🎮 遊戲應用：\n• 遊戲 AI 最常用的尋路演算法\n• Unity NavMeshAgent 底層原理\n• MOBA 角色移動路徑計算\n• 即時戰略的多單位尋路\n\n紫 = Open，橙 = Closed\n按 E 執行 A*";
        formula = "f(n) = g(n) + h(n)    h = |dx| + |dy|";
        challengeDescription = "完整執行一次 A*";

        var gridGo = new GameObject("AStarGrid");
        gridGo.transform.SetParent(transform, false);
        gridGo.transform.localPosition = Vector3.zero;
        grid = gridGo.AddComponent<PathfindingGrid>();
        grid.BuildGrid(gridGo.transform);

        statusLabel = CreateLabel(new Vector3(0, -4.5f, 0), "按 E 執行 A*", 32, Color.white);
        statsLabel = CreateLabel(new Vector3(0, -5.2f, 0), "f = g + h    h = 曼哈頓距離（啟發式）", 26, new Color(0.7f, 0.8f, 1f));
    }

    public override void UpdateVisualization() { }

    protected override void OnChallengeStart()
    {
        if (isRunning) return;
        isRunning = true;
        grid.ResetPathData();
        grid.UpdateVisuals();
        statusLabel.text = "A* 執行中...";
        statusLabel.color = new Color(0.8f, 0.5f, 1f);
        StartCoroutine(RunAStar());
    }

    private IEnumerator RunAStar()
    {
        explored = 0;
        var start = grid.StartCell;
        var end = grid.EndCell;

        // 簡易 open set（按 f 排序）
        var open = new SortedList<int, List<Vector2Int>>();

        var sc = grid.cells[start.x, start.y];
        sc.gCost = 0;
        sc.hCost = PathfindingGrid.Manhattan(start, end);
        grid.cells[start.x, start.y] = sc;
        AddToOpen(open, sc.gCost + sc.hCost, start);

        bool found = false;

        while (open.Count > 0)
        {
            var firstKey = open.Keys[0];
            var list = open[firstKey];
            var current = list[0];
            list.RemoveAt(0);
            if (list.Count == 0) open.Remove(firstKey);

            if (grid.cells[current.x, current.y].inClosed) continue;

            var cc = grid.cells[current.x, current.y];
            cc.inClosed = true;
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
                    nc.hCost = PathfindingGrid.Manhattan(nb, end);
                    nc.parentX = current.x;
                    nc.parentY = current.y;
                    nc.inOpen = true;
                    grid.cells[nb.x, nb.y] = nc;
                    AddToOpen(open, newG + nc.hCost, nb);
                }
            }

            if (explored % 2 == 0)
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
        statusLabel.text = found ? $"✓ 找到！探索僅 {explored} 格，成本 {totalCost}" : "✗ 無路可達";
        statusLabel.color = found ? new Color(0.3f, 1f, 0.5f) : new Color(1f, 0.4f, 0.3f);
        statsLabel.text = $"A* 特性：啟發式引導 → 探索大幅減少！";

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
