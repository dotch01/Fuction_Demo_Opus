using System.Collections;
using UnityEngine;

/// <summary>
/// 定時在地圖範圍內隨機生成道具，保持遊戲趣味性。
/// </summary>
public class ItemSpawner : MonoBehaviour
{
    private float _arenaWidth;
    private float _arenaHeight;
    private float _spawnInterval;
    private int _maxItems;

    private Sprite _circleSprite;
    private Sprite _diamondSprite;
    private Sprite _squareSprite;

    public void Init(float arenaWidth, float arenaHeight,
        Sprite circleSprite, Sprite diamondSprite, Sprite squareSprite,
        float spawnInterval = 4f, int maxItems = 30)
    {
        _arenaWidth = arenaWidth;
        _arenaHeight = arenaHeight;
        _circleSprite = circleSprite;
        _diamondSprite = diamondSprite;
        _squareSprite = squareSprite;
        _spawnInterval = spawnInterval;
        _maxItems = maxItems;

        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(_spawnInterval);

            // 限制場上道具數量
            var existing = FindObjectsByType<Collectible>(FindObjectsSortMode.None);
            if (existing.Length >= _maxItems) continue;

            var config = ItemConfigLoader.Load();
            if (config == null || config.items.Count == 0) continue;

            // 隨機選一個道具
            ItemEntry entry = config.items[Random.Range(0, config.items.Count)];
            SpawnOne(entry);
        }
    }

    private void SpawnOne(ItemEntry entry)
    {
        float margin = 1.5f;
        float xRange = _arenaWidth / 2 - margin;
        float yRange = _arenaHeight / 2 - margin;
        float x = Random.Range(-xRange, xRange);
        float y = Random.Range(-yRange, yRange);

        // 避開玩家附近
        var pc = Demo2PlayerController.Instance;
        if (pc != null)
        {
            Vector2 ppos = pc.Position;
            if (Vector2.Distance(new Vector2(x, y), ppos) < 2f)
                x += (x > ppos.x ? 3f : -3f);
        }

        var go = new GameObject($"Item_{entry.id}_spawned");
        go.transform.position = new Vector3(x, y, 0);

        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;
        col.isTrigger = true;

        Sprite sprite = entry.spriteName switch
        {
            "diamond" => _diamondSprite,
            "square" => _squareSprite,
            _ => _circleSprite
        };

        var spriteRoot = new GameObject("Sprite");
        spriteRoot.transform.SetParent(go.transform, false);
        spriteRoot.transform.localScale = Vector3.one * entry.spriteScale;

        var sr = spriteRoot.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = entry.color.ToColor();
        sr.sortingOrder = 3;

        // 光暈
        var glow = new GameObject("Glow");
        glow.transform.SetParent(spriteRoot.transform, false);
        glow.transform.localScale = Vector3.one * 1.8f;
        var glowSR = glow.AddComponent<SpriteRenderer>();
        glowSR.sprite = _circleSprite;
        Color c = entry.color.ToColor();
        glowSR.color = new Color(c.r, c.g, c.b, 0.15f);
        glowSR.sortingOrder = 2;

        // 出現動畫：從小放大
        go.transform.localScale = Vector3.zero;
        var collectible = go.AddComponent<Collectible>();
        collectible.Init(entry, spriteRoot.transform);

        StartCoroutine(ScaleIn(go.transform));
    }

    private IEnumerator ScaleIn(Transform t)
    {
        float duration = 0.3f;
        float elapsed = 0f;
        while (elapsed < duration && t != null)
        {
            elapsed += Time.deltaTime;
            float s = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            t.localScale = Vector3.one * s;
            yield return null;
        }
        if (t != null) t.localScale = Vector3.one;
    }
}
