using System;
using UnityEngine;

/// <summary>
/// 玩家 HP / MP 狀態管理。
/// </summary>
public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    public event Action OnStatsChanged;

    [Header("HP")]
    public int MaxHP = 100;
    public int CurrentHP { get; private set; }

    [Header("MP")]
    public int MaxMP = 60;
    public int CurrentMP { get; private set; }

    public bool IsDead => CurrentHP <= 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        CurrentHP = MaxHP;
        CurrentMP = MaxMP;
    }

    public void ModifyHP(int amount)
    {
        CurrentHP = Mathf.Clamp(CurrentHP + amount, 0, MaxHP);
        Debug.Log($"[PlayerStats] HP: {CurrentHP}/{MaxHP} ({(amount >= 0 ? "+" : "")}{amount})");
        OnStatsChanged?.Invoke();
    }

    public void ModifyMP(int amount)
    {
        CurrentMP = Mathf.Clamp(CurrentMP + amount, 0, MaxMP);
        Debug.Log($"[PlayerStats] MP: {CurrentMP}/{MaxMP} ({(amount >= 0 ? "+" : "")}{amount})");
        OnStatsChanged?.Invoke();
    }

    /// <summary>
    /// 套用 ItemEffect，回傳是否成功。
    /// </summary>
    public bool ApplyEffect(ItemEffect effect)
    {
        if (effect == null) return false;

        switch (effect.type)
        {
            case "RestoreHP":
                if (CurrentHP >= MaxHP) return false;
                ModifyHP(effect.value);
                return true;
            case "RestoreMP":
                if (CurrentMP >= MaxMP) return false;
                ModifyMP(effect.value);
                return true;
            case "DamageHP":
                ModifyHP(-effect.value);
                return true;
            case "DamageMP":
                ModifyMP(-effect.value);
                return true;
            default:
                Debug.LogWarning($"[PlayerStats] Unknown effect: {effect.type}");
                return false;
        }
    }
}
