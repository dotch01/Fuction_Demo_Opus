using System;
using UnityEngine;

/// <summary>
/// JSON 行星設定檔的 C# 資料結構。
/// 全部 [Serializable] 以支援 JsonUtility。
/// </summary>
/// 
[Serializable]
public class PlanetConfig
{
    public StarSystemEntry[] starSystems;
    public PlanetEntry[] planets;
}

[Serializable]
public class StarSystemEntry
{
    public string name;
    public int quadrant;
    public Vec2 center;
    public float radius;
}

[Serializable]
public class PlanetEntry
{
    public string id;
    public string defaultName;
    public string description;
    public Vec2 position;
    public float scale;
    public int radius;
    public int mass;
    public int temperature;
    public int waterPercent;
    public float earthSimilarity;
    public bool isQuestTarget;
    public bool filterOnly;
    public int questOrder; // 0=非任務, 1~8=線性任務順序
}

[Serializable]
public class Vec2
{
    public float x;
    public float y;

    public Vector2 ToVector2() => new(x, y);
}
