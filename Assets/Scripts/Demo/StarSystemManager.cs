using UnityEngine;

/// <summary>
/// 星系管理器：管理星系列表，根據座標判斷所屬星系。
/// 星系由 JSON 定義範圍+名稱，行星根據位置自動歸屬。
/// </summary>
public class StarSystemManager : MonoBehaviour
{
    public static StarSystemManager Instance { get; private set; }

    private StarSystemEntry[] _systems;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Init(StarSystemEntry[] systems)
    {
        _systems = systems;
    }

    /// <summary>根據世界座標取得所在星系名稱，不在任何星系內則回傳 null。</summary>
    public string GetStarSystemName(Vector2 pos)
    {
        var entry = GetStarSystem(pos);
        return entry?.name;
    }

    /// <summary>根據世界座標取得所在星系。</summary>
    public StarSystemEntry GetStarSystem(Vector2 pos)
    {
        if (_systems == null) return null;

        foreach (var sys in _systems)
        {
            Vector2 center = sys.center.ToVector2();
            if (Vector2.Distance(pos, center) <= sys.radius)
                return sys;
        }

        return null;
    }

    /// <summary>根據行星位置取得歸屬星系名稱。</summary>
    public string GetStarSystemForPlanet(PlanetEntry planet)
    {
        return GetStarSystemName(planet.position.ToVector2());
    }
}
