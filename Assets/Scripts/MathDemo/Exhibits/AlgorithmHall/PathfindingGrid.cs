using System.Collections.Generic;
using UnityEngine;

// ============================================================
// PathfindingGrid.cs — 15×15 共用網格地圖
// BFS / Dijkstra / A* 展品共用此元件
// ============================================================

public class PathfindingGrid : MonoBehaviour
{
    public int width = 15;
    public int height = 15;
    public float cellSize = 0.45f;

    public struct Cell
    {
        public bool walkable;
        public int weight;       // 1=平地, 2=沼澤, 3=山地
        public int gCost, hCost; // pathfinding
        public int parentX, parentY;
        public bool visited;
        public bool inPath;
        public bool inOpen;
        public bool inClosed;
    }

    public Cell[,] cells;
    private GameObject[,] cellObjects;
    private Renderer[,] cellRenderers;

    private Vector2Int startCell = new Vector2Int(1, 1);
    private Vector2Int endCell = new Vector2Int(13, 13);

    // 顏色定義
    private static readonly Color colorWalkable   = new Color(0.2f, 0.22f, 0.28f);
    private static readonly Color colorWall       = new Color(0.12f, 0.12f, 0.14f);
    private static readonly Color colorWeight2    = new Color(0.35f, 0.35f, 0.18f);
    private static readonly Color colorWeight3    = new Color(0.4f, 0.2f, 0.15f);
    private static readonly Color colorStart      = new Color(0.2f, 0.8f, 0.3f);
    private static readonly Color colorEnd        = new Color(0.8f, 0.2f, 0.2f);
    private static readonly Color colorVisited    = new Color(0.25f, 0.4f, 0.6f);
    private static readonly Color colorOpen       = new Color(0.5f, 0.3f, 0.7f);
    private static readonly Color colorClosed     = new Color(0.35f, 0.25f, 0.5f);
    private static readonly Color colorPath       = new Color(1f, 0.85f, 0.2f);

    public Vector2Int StartCell { get => startCell; set => startCell = value; }
    public Vector2Int EndCell { get => endCell; set => endCell = value; }
    public int FCost(int x, int y) => cells[x, y].gCost + cells[x, y].hCost;

    public void BuildGrid(Transform parent)
    {
        cells = new Cell[width, height];
        cellObjects = new GameObject[width, height];
        cellRenderers = new Renderer[width, height];

        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));

        float offsetX = -(width * cellSize) / 2f;
        float offsetZ = -(height * cellSize) / 2f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cells[x, y] = new Cell { walkable = true, weight = 1 };

                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = $"Cell_{x}_{y}";
                go.transform.SetParent(parent, false);
                go.transform.localPosition = new Vector3(
                    offsetX + x * cellSize + cellSize * 0.5f,
                    0,
                    offsetZ + y * cellSize + cellSize * 0.5f
                );
                go.transform.localScale = new Vector3(cellSize * 0.9f, 0.1f, cellSize * 0.9f);

                var r = go.GetComponent<Renderer>();
                r.material = new Material(mat);
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                var col = go.GetComponent<Collider>();
                if (col != null) Object.Destroy(col);

                cellObjects[x, y] = go;
                cellRenderers[x, y] = r;
            }
        }

        GenerateDefaultMap();
        UpdateVisuals();
    }

    public void GenerateDefaultMap()
    {
        // 外牆 + 隨機障礙 + 地形
        var rng = new System.Random(42);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cells[x, y].walkable = true;
                cells[x, y].weight = 1;

                // 隨機牆壁 (25%) — 避開起終點
                if (rng.NextDouble() < 0.25f)
                {
                    if (!IsStartOrEnd(x, y) && !IsAdjacentToStartEnd(x, y))
                        cells[x, y].walkable = false;
                }
                // 隨機地形權重
                else if (rng.NextDouble() < 0.2f)
                    cells[x, y].weight = 2;
                else if (rng.NextDouble() < 0.1f)
                    cells[x, y].weight = 3;
            }
        }

        // 確保起終點可走
        cells[startCell.x, startCell.y].walkable = true;
        cells[startCell.x, startCell.y].weight = 1;
        cells[endCell.x, endCell.y].walkable = true;
        cells[endCell.x, endCell.y].weight = 1;
    }

    private bool IsStartOrEnd(int x, int y) =>
        (x == startCell.x && y == startCell.y) || (x == endCell.x && y == endCell.y);

    private bool IsAdjacentToStartEnd(int x, int y)
    {
        return Mathf.Abs(x - startCell.x) + Mathf.Abs(y - startCell.y) <= 1 ||
               Mathf.Abs(x - endCell.x) + Mathf.Abs(y - endCell.y) <= 1;
    }

    public void ResetPathData()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var c = cells[x, y];
                c.gCost = int.MaxValue;
                c.hCost = 0;
                c.parentX = -1;
                c.parentY = -1;
                c.visited = false;
                c.inPath = false;
                c.inOpen = false;
                c.inClosed = false;
                cells[x, y] = c;
            }
        }
    }

    public void UpdateVisuals()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color c;
                if (x == startCell.x && y == startCell.y) c = colorStart;
                else if (x == endCell.x && y == endCell.y) c = colorEnd;
                else if (cells[x, y].inPath) c = colorPath;
                else if (cells[x, y].inOpen) c = colorOpen;
                else if (cells[x, y].inClosed) c = colorClosed;
                else if (cells[x, y].visited) c = colorVisited;
                else if (!cells[x, y].walkable) c = colorWall;
                else if (cells[x, y].weight >= 3) c = colorWeight3;
                else if (cells[x, y].weight >= 2) c = colorWeight2;
                else c = colorWalkable;

                cellRenderers[x, y].material.color = c;

                // 牆壁稍高
                float h = cells[x, y].walkable ? 0.1f : 0.35f;
                var s = cellObjects[x, y].transform.localScale;
                s.y = h;
                cellObjects[x, y].transform.localScale = s;
            }
        }
    }

    public List<Vector2Int> GetNeighbors(int x, int y)
    {
        var list = new List<Vector2Int>(4);
        if (x > 0 && cells[x - 1, y].walkable) list.Add(new Vector2Int(x - 1, y));
        if (x < width - 1 && cells[x + 1, y].walkable) list.Add(new Vector2Int(x + 1, y));
        if (y > 0 && cells[x, y - 1].walkable) list.Add(new Vector2Int(x, y - 1));
        if (y < height - 1 && cells[x, y + 1].walkable) list.Add(new Vector2Int(x, y + 1));
        return list;
    }

    public List<Vector2Int> TracePath(Vector2Int end)
    {
        var path = new List<Vector2Int>();
        int cx = end.x, cy = end.y;
        int safety = 300;
        while (cx >= 0 && cy >= 0 && safety-- > 0)
        {
            path.Add(new Vector2Int(cx, cy));
            var c = cells[cx, cy];
            if (c.parentX < 0) break;
            int px = c.parentX, py = c.parentY;
            cx = px; cy = py;
        }
        path.Reverse();
        return path;
    }

    public void MarkPath(List<Vector2Int> path)
    {
        foreach (var p in path)
        {
            var c = cells[p.x, p.y];
            c.inPath = true;
            cells[p.x, p.y] = c;
        }
    }

    public static int Manhattan(Vector2Int a, Vector2Int b) =>
        Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
}
