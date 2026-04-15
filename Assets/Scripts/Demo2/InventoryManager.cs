using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 背包管理器：支援多種道具分類（貨幣/消耗品/任務/收藏品），
/// 可堆疊道具，支援使用消耗品。
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public event Action OnInventoryChanged;

    /// <summary>背包中的道具格子。</summary>
    public class ItemSlot
    {
        public ItemEntry Entry;
        public int Count;
    }

    private readonly List<ItemSlot> _slots = new();
    public IReadOnlyList<ItemSlot> Slots => _slots;

    public int TotalScore { get; private set; }
    public int TotalItems { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// 加入道具到背包。可堆疊的道具會合併到已有格子。
    /// </summary>
    public void AddItem(ItemEntry entry, int count = 1)
    {
        TotalScore += entry.scoreValue * count;
        TotalItems += count;

        if (entry.stackable)
        {
            var existing = _slots.Find(s => s.Entry.id == entry.id);
            if (existing != null)
            {
                existing.Count = Mathf.Min(existing.Count + count, entry.maxStack);
                OnInventoryChanged?.Invoke();
                return;
            }
        }

        _slots.Add(new ItemSlot { Entry = entry, Count = count });

        Debug.Log($"[Inventory] +{count} {entry.name} (Total: {TotalItems} items, {TotalScore} pts)");
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// 使用消耗品。回傳是否使用成功。
    /// </summary>
    public bool UseItem(ItemSlot slot)
    {
        if (slot == null || slot.Count <= 0) return false;
        if (slot.Entry.category != "Consumable") return false;
        if (slot.Entry.effect == null) return false;

        if (PlayerStats.Instance == null) return false;
        bool applied = PlayerStats.Instance.ApplyEffect(slot.Entry.effect);
        if (!applied) return false;

        slot.Count--;
        if (slot.Count <= 0)
            _slots.Remove(slot);

        OnInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// 取得特定分類的道具格子。
    /// </summary>
    public List<ItemSlot> GetSlotsByCategory(string category)
    {
        return _slots.FindAll(s => s.Entry.category == category);
    }

    /// <summary>
    /// 取得特定 ID 的道具數量。
    /// </summary>
    public int GetItemCount(string itemId)
    {
        var slot = _slots.Find(s => s.Entry.id == itemId);
        return slot?.Count ?? 0;
    }
}
