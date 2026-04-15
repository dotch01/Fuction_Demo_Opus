using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 行星工廠：根據 PlanetEntry[] 建立行星 GameObject。
/// 分為普通行星和濾鏡行星兩組。取代 StarSpawner。
/// </summary>
public class PlanetFactory : MonoBehaviour
{
    public static PlanetFactory Instance { get; private set; }

    private readonly List<Star> _normalStars = new();
    private readonly List<Star> _filterStars = new();

    [Header("世界邊界 (與 CameraController 一致)")]
    [SerializeField] private float worldWidth = 100f;
    [SerializeField] private float worldHeight = 100f;

    private Sprite _circleSprite;

    public IReadOnlyList<Star> NormalStars => _normalStars;
    public IReadOnlyList<Star> FilterStars => _filterStars;

    /// <summary>目前可見的行星列表（根據濾鏡狀態）。</summary>
    public IReadOnlyList<Star> ActiveStars =>
        FilterManager.Instance != null && FilterManager.Instance.IsFilterActive ? _filterStars : _normalStars;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _circleSprite = CreateCircleSprite();
    }

    public void BuildFromConfig(PlanetConfig config)
    {
        foreach (var entry in config.planets)
        {
            var star = CreateStar(entry);
            if (entry.filterOnly)
                _filterStars.Add(star);
            else
                _normalStars.Add(star);
        }

        // 預設隱藏濾鏡行星
        SetFilterVisibility(false);

        Debug.Log($"PlanetFactory: {_normalStars.Count} normal, {_filterStars.Count} filter planets created");
    }

    public void SetFilterVisibility(bool filterOn)
    {
        foreach (var s in _normalStars)
            s.gameObject.SetActive(!filterOn);
        foreach (var s in _filterStars)
            s.gameObject.SetActive(filterOn);
    }

    private Star CreateStar(PlanetEntry entry)
    {
        var go = new GameObject(string.IsNullOrEmpty(entry.defaultName) ? entry.id : entry.defaultName);
        go.transform.SetParent(transform);
        go.transform.position = new Vector3(entry.position.x, entry.position.y, 0f);
        go.transform.localScale = Vector3.one * entry.scale;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = _circleSprite;
        sr.color = GetStarColor(entry);
        sr.sortingOrder = 1;

        var star = go.AddComponent<Star>();
        star.Init(entry);

        return star;
    }

    // ──────────────── 無縫世界 Wrap ────────────────

    /// <summary>
    /// 每幀把所有星星的 transform.position 調整到距離鏡頭最近的「副本」位置。
    /// 這樣鏡頭不管移到哪裡，星星都會出現在正確的相對位置，完全無縫。
    /// </summary>
    private void LateUpdate()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 camPos = cam.transform.position;

        foreach (var star in _normalStars)
            WrapStarPosition(star, camPos);
        foreach (var star in _filterStars)
            WrapStarPosition(star, camPos);
    }

    private void WrapStarPosition(Star star, Vector3 camPos)
    {
        Vector2 orig = star.OriginalPosition;
        // Round 找到最近的副本偏移量
        float ox = Mathf.Round((camPos.x - orig.x) / worldWidth) * worldWidth;
        float oy = Mathf.Round((camPos.y - orig.y) / worldHeight) * worldHeight;
        star.transform.position = new Vector3(orig.x + ox, orig.y + oy, 0f);
    }

    private static Color GetStarColor(PlanetEntry entry)
    {
        if (entry.isQuestTarget)
        {
            // 任務行星用稍微偏青色讓玩家感覺不同
            return new Color(0.6f, 0.9f, 1f, 1f);
        }

        float h = Mathf.Repeat(entry.id.GetHashCode() * 0.0001f, 1f);
        float s = Mathf.Lerp(0f, 0.2f, Mathf.Repeat(entry.position.x * 0.1f, 1f));
        float v = Mathf.Lerp(0.7f, 1f, Mathf.Repeat(entry.position.y * 0.1f, 1f));
        return Color.HSVToRGB(h, s, v);
    }

    private static Sprite CreateCircleSprite()
    {
        const int size = 32;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        Vector2 center = new(size * 0.5f, size * 0.5f);
        float radius = size * 0.5f;

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dist = Vector2.Distance(new Vector2(x, y), center);
            float alpha = Mathf.Clamp01((radius - dist) * 2f);
            tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
