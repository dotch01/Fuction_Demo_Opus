using UnityEngine;

/// <summary>
/// 可收集物品：玩家碰觸後自動收集，帶浮動 + 旋轉動畫。
/// 透過 ItemEntry（JSON 驅動）初始化。
/// </summary>
public class Collectible : MonoBehaviour
{
    public ItemEntry Entry { get; private set; }

    private Transform _spriteRoot;
    private float _bobOffset;
    private float _bobSpeed;
    private float _rotSpeed;
    private Vector3 _basePos;
    private bool _collected;

    // 收集動畫
    private float _collectTimer;
    private const float CollectDuration = 0.3f;

    public void Init(ItemEntry entry, Transform spriteRoot)
    {
        Entry = entry;
        _spriteRoot = spriteRoot;
        _basePos = transform.position;

        _bobOffset = Random.Range(0f, Mathf.PI * 2f);
        _bobSpeed = Random.Range(1.5f, 2.5f);
        _rotSpeed = Random.Range(30f, 90f);
    }

    private void Update()
    {
        if (_collected)
        {
            PlayCollectAnimation();
            return;
        }

        // 上下浮動
        float bob = Mathf.Sin(Time.time * _bobSpeed + _bobOffset) * 0.15f;
        transform.position = _basePos + new Vector3(0, bob, 0);

        // 緩慢旋轉
        if (_spriteRoot != null)
            _spriteRoot.Rotate(0, 0, _rotSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_collected) return;

        var player = other.GetComponent<Demo2PlayerController>();
        if (player == null) return;

        _collected = true;
        _collectTimer = 0f;

        // 有即時效果（如毒霧、魔力虹吸）的道具直接生效，不進背包
        bool instantEffect = Entry.effect != null &&
            (Entry.effect.type == "DamageHP" || Entry.effect.type == "DamageMP");

        if (instantEffect)
        {
            if (PlayerStats.Instance != null)
                PlayerStats.Instance.ApplyEffect(Entry.effect);
        }
        else if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(Entry);
        }
    }

    private void PlayCollectAnimation()
    {
        _collectTimer += Time.deltaTime;
        float t = _collectTimer / CollectDuration;

        float scale = Mathf.Lerp(1f, 0f, t * t);
        transform.localScale = Vector3.one * scale;
        transform.position += Vector3.up * (8f * Time.deltaTime);

        if (t >= 1f)
            Destroy(gameObject);
    }
}
