using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 道具資料定義（對應 ItemConfig.json）。
/// </summary>
[Serializable]
public class ItemConfig
{
    public List<ItemEntry> items;
}

[Serializable]
public class ItemEntry
{
    public string id;
    public string name;
    public string category;   // Currency, Consumable, Quest, Collection
    public string description;
    public int scoreValue;
    public bool stackable;
    public int maxStack;
    public string spriteName; // circle, diamond, square
    public ColorEntry color;
    public float spriteScale;
    public ItemEffect effect;
}

[Serializable]
public class ColorEntry
{
    public float r, g, b, a;
    public Color ToColor() => new(r, g, b, a);
}

[Serializable]
public class ItemEffect
{
    public string type;  // RestoreHP, RestoreMP, DamageHP, DamageMP
    public int value;
}

/// <summary>
/// 道具分類列舉。
/// </summary>
public enum ItemCategory
{
    Currency,
    Consumable,
    Quest,
    Collection
}

/// <summary>
/// 效果類型列舉。
/// </summary>
public enum EffectType
{
    None,
    RestoreHP,
    RestoreMP,
    DamageHP,
    DamageMP
}
