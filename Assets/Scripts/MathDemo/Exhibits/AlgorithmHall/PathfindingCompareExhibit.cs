using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ============================================================
// PathfindingCompareExhibit.cs — 尋路對比
// BFS / Dijkstra / A* 三合一同時執行比較
// ============================================================

public class PathfindingCompareExhibit : ExhibitBase
{
    private PathfindingGrid gridBFS, gridDijk, gridAStar;
    private TextMesh statusLabel;
    private TextMesh bfsResult, dijkResult, astarResult;
    private bool isRunning;

    public override void BuildExhibit()
    {
        exhibitName = "尋路對比 Pathfinding Compare";
        description = "三種尋路同時在相同地圖上執行：\n\n🔵 BFS — 最短步數（不看權重）\n🟢 Dijkstra — 最小成本（看權重）\n🟣 A*  — 最小成本 + 啟發式（最快）\n\n🎮 實際選擇：\n• 均勻格子地圖 → BFS 就夠\n• 有地形權重 → Dijkstra\n• 大地圖+效能要求 → A*（探索量最少）\n\n按 E 同時啟動三個比較！";
        formula = "BFS: O(V+E)    Dijkstra: O((V+E)logV)    A*: O(E)平均";
        challengeDescription = "完整觀看三演算法對比";

        float gridSpacing = 3.2f;

        // 三張地圖並排
        var g1 = new GameObject("Grid_BFS");
        g1.transform.SetParent(transform, false);
        g1.transform.localPosition = new Vector3(-gridSpacing, 0, 0);
        gridBFS = g1.AddComponent<PathfindingGrid>();
        gridBFS.width = 10; gridBFS.height = 10; gridBFS.cellSize = 0.28f;
        gridBFS.StartCell = new Vector2Int(1, 1);
        gridBFS.EndCell = new Vector2Int(8, 8);
        gridBFS.BuildGrid(g1.transform);

        var g2 = new GameObject("Grid_Dijkstra");
        g2.transform.SetParent(transform, false);
        g2.transform.localPosition = new Vector3(0, 0, 0);
        gridDijk = g2.AddComponent<PathfindingGrid>();
        gridDijk.width = 10; gridDijk.height = 10; gridDijk.cellSize = 0.28f;
        gridDijk.StartCell = new Vector2Int(1, 1);
        gridDijk.EndCell = new Vector2Int(8, 8);
        gridDijk.BuildGrid(g2.transform);

        var g3 = new GameObject("Grid_AStar");
        g3.transform.SetParent(transform, false);
        g3.transform.localPosition = new Vector3(gridSpacing, 0, 0);
        gridAStar = g3.AddComponent<PathfindingGrid>();
        gridAStar.width = 10; gridAStar.height = 10; gridAStar.cellSize = 0.28f;
        gridAStar.StartCell = new Vector2Int(1, 1);
        gridAStar.EndCell = new Vector2Int(8, 8);
        gridAStar.BuildGrid(g3.transform);

        // 同步地圖（複製 BFS 的牆壁和權重到另外兩張）
        SyncGrids();

        CreateLabel(new Vector3(-gridSpacing, 2f, 0), "BFS", 30, new Color(0.3f, 0.7f, 1f));
        CreateLabel(new Vector3(0, 2f, 0), "Dijkstra", 30, new Color(0.3f, 0.9f, 0.5f));
        CreateLabel(new Vector3(gridSpacing, 2f, 0), "A*", 30, new Color(0.7f, 0.4f, 1f));

        statusLabel = CreateLabel(new Vector3(0, -2.5f, 0), "按 E 同時啟動三個", 32, Color.white);
        bfsResult = CreateLabel(new Vector3(-gridSpacing, -2f, 0), "", 22, new Color(0.3f, 0.7f, 1f));
        dijkResult = CreateLabel(new Vector3(0, -2f, 0), "", 22, new Color(0.3f, 0.9f, 0.5f));
        astarResult = CreateLabel(new Vector3(gridSpacing, -2f, 0), "", 22, new Color(0.7f, 0.4f, 1f));
    }

    private void SyncGrids()
    {
        int w = gridBFS.width, h = gridBFS.height;
        gridDijk.StartCell = gridBFS.StartCell;
        gridDijk.EndCell = gridBFS.EndCell;
        gridAStar.StartCell = gridBFS.StartCell;
        gridAStar.EndCell = gridBFS.EndCell;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                gridDijk.cells[x, y].walkable = gridBFS.cells[x, y].walkable;
                gridDijk.cells[x, y].weight = gridBFS.cells[x, y].weight;
                gridAStar.cells[x, y].walkable = gridBFS.cells[x, y].walkable;
                gridAStar.cells[x, y].weight = gridBFS.cells[x, y].weight;
            }
        }
        gridDijk.UpdateVisuals();
        gridAStar.UpdateVisuals();
    }

    public override void UpdateVisualization() { }

    protected override void OnChallengeStart()
    {
        if (isRunning) return;
        isRunning = true;

        // 重新生成地圖
        gridBFS.GenerateDefaultMap();
        gridBFS.UpdateVisuals();
        SyncGrids();

        gridBFS.ResetPathData(); gridBFS.UpdateVisuals();
        gridDijk.ResetPathData(); gridDijk.UpdateVisuals();
        gridAStar.ResetPathData(); gridAStar.UpdateVisuals();

        statusLabel.text = "同時執行中...";
        statusLabel.color = new Color(1f, 0.85f, 0.3f);

        StartCoroutine(RunAll());
    }

    private IEnumerator RunAll()
    {
        var c1 = StartCoroutine(RunBFS());
        var c2 = StartCoroutine(RunDijkstra());
        var c3 = StartCoroutine(RunAStar());

        yield return c1;
        yield return c2;
        yield return c3;

        statusLabel.text = "✓ 對比完成！按 E 重新對比";
        statusLabel.color = new Color(0.3f, 1f, 0.5f);
        isRunning = false;
        challengeCompleted = true;
        ChallengeSystem.Instance?.MarkCompleted(exhibitName);
    }

    private IEnumerator RunBFS()
    {
        int explored = 0;
        var start = gridBFS.StartCell;
        var end = gridBFS.EndCell;
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(start);
        var sc = gridBFS.cells[start.x, start.y]; sc.gCost = 0; sc.visited = true; gridBFS.cells[start.x, start.y] = sc;
        bool found = false;

        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            explored++;
            if (cur == end) { found = true; break; }

            foreach (var nb in gridBFS.GetNeighbors(cur.x, cur.y))
            {
                if (gridBFS.cells[nb.x, nb.y].visited) continue;
                var nc = gridBFS.cells[nb.x, nb.y]; nc.visited = true; nc.parentX = cur.x; nc.parentY = cur.y;
                nc.gCost = gridBFS.cells[cur.x, cur.y].gCost + 1; gridBFS.cells[nb.x, nb.y] = nc;
                queue.Enqueue(nb);
            }
            if (explored % 3 == 0) { gridBFS.UpdateVisuals(); yield return new WaitForSeconds(0.03f); }
        }

        if (found) { var path = gridBFS.TracePath(end); gridBFS.MarkPath(path); }
        gridBFS.UpdateVisuals();
        bfsResult.text = $"探索 {explored} 格";
    }

    private IEnumerator RunDijkstra()
    {
        int explored = 0;
        var start = gridDijk.StartCell;
        var end = gridDijk.EndCell;
        var open = new SortedList<int, List<Vector2Int>>();
        var sc = gridDijk.cells[start.x, start.y]; sc.gCost = 0; gridDijk.cells[start.x, start.y] = sc;
        AddOpen(open, 0, start);
        bool found = false;

        while (open.Count > 0)
        {
            var fk = open.Keys[0]; var l = open[fk]; var cur = l[0]; l.RemoveAt(0); if (l.Count == 0) open.Remove(fk);
            if (gridDijk.cells[cur.x, cur.y].inClosed) continue;
            var cc = gridDijk.cells[cur.x, cur.y]; cc.inClosed = true; cc.visited = true; gridDijk.cells[cur.x, cur.y] = cc;
            explored++;
            if (cur == end) { found = true; break; }

            foreach (var nb in gridDijk.GetNeighbors(cur.x, cur.y))
            {
                if (gridDijk.cells[nb.x, nb.y].inClosed) continue;
                int ng = gridDijk.cells[cur.x, cur.y].gCost + gridDijk.cells[nb.x, nb.y].weight;
                if (ng < gridDijk.cells[nb.x, nb.y].gCost)
                {
                    var nc = gridDijk.cells[nb.x, nb.y]; nc.gCost = ng; nc.parentX = cur.x; nc.parentY = cur.y; nc.inOpen = true;
                    gridDijk.cells[nb.x, nb.y] = nc; AddOpen(open, ng, nb);
                }
            }
            if (explored % 3 == 0) { gridDijk.UpdateVisuals(); yield return new WaitForSeconds(0.03f); }
        }

        if (found) { var path = gridDijk.TracePath(end); gridDijk.MarkPath(path); }
        gridDijk.UpdateVisuals();
        dijkResult.text = $"探索 {explored} 格";
    }

    private IEnumerator RunAStar()
    {
        int explored = 0;
        var start = gridAStar.StartCell;
        var end = gridAStar.EndCell;
        var open = new SortedList<int, List<Vector2Int>>();
        var sc = gridAStar.cells[start.x, start.y]; sc.gCost = 0; sc.hCost = PathfindingGrid.Manhattan(start, end);
        gridAStar.cells[start.x, start.y] = sc;
        AddOpen(open, sc.gCost + sc.hCost, start);
        bool found = false;

        while (open.Count > 0)
        {
            var fk = open.Keys[0]; var l = open[fk]; var cur = l[0]; l.RemoveAt(0); if (l.Count == 0) open.Remove(fk);
            if (gridAStar.cells[cur.x, cur.y].inClosed) continue;
            var cc = gridAStar.cells[cur.x, cur.y]; cc.inClosed = true; gridAStar.cells[cur.x, cur.y] = cc;
            explored++;
            if (cur == end) { found = true; break; }

            foreach (var nb in gridAStar.GetNeighbors(cur.x, cur.y))
            {
                if (gridAStar.cells[nb.x, nb.y].inClosed) continue;
                int ng = gridAStar.cells[cur.x, cur.y].gCost + gridAStar.cells[nb.x, nb.y].weight;
                if (ng < gridAStar.cells[nb.x, nb.y].gCost)
                {
                    var nc = gridAStar.cells[nb.x, nb.y]; nc.gCost = ng; nc.hCost = PathfindingGrid.Manhattan(nb, end);
                    nc.parentX = cur.x; nc.parentY = cur.y; nc.inOpen = true;
                    gridAStar.cells[nb.x, nb.y] = nc; AddOpen(open, ng + nc.hCost, nb);
                }
            }
            if (explored % 2 == 0) { gridAStar.UpdateVisuals(); yield return new WaitForSeconds(0.03f); }
        }

        if (found) { var path = gridAStar.TracePath(end); gridAStar.MarkPath(path); }
        gridAStar.UpdateVisuals();
        astarResult.text = $"探索 {explored} 格";
    }

    private void AddOpen(SortedList<int, List<Vector2Int>> o, int k, Vector2Int p)
    {
        if (!o.ContainsKey(k)) o[k] = new List<Vector2Int>();
        o[k].Add(p);
    }

    public override bool CheckChallengeComplete() => challengeCompleted;
}
