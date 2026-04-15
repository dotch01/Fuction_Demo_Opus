using UnityEngine;

/// <summary>
/// 濾鏡管理器：切換普通/濾鏡行星的可見性。
/// 透過 PlanetFactory 控制顯示哪一組。
/// </summary>
public class FilterManager : MonoBehaviour
{
    public static FilterManager Instance { get; private set; }

    public bool IsFilterActive { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Toggle()
    {
        IsFilterActive = !IsFilterActive;
        if (PlanetFactory.Instance != null)
            PlanetFactory.Instance.SetFilterVisibility(IsFilterActive);
        Debug.Log(IsFilterActive ? "濾鏡開啟" : "濾鏡關閉");
    }
}
