using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD 管理器：即時更新螢幕上的座標、象限、星系名稱。
/// </summary>
public class HUDManager : MonoBehaviour
{
    [Header("UI 參照")]
    [SerializeField] private Text positionText;
    [SerializeField] private Text quadrantText;
    [SerializeField] private Text starSystemText;

    private void Update()
    {
        if (QuadrantManager.Instance == null) return;

        Vector2 pos = QuadrantManager.Instance.CameraPosition;
        positionText.text = $"( {pos.x:F2} , {pos.y:F2} )";
        quadrantText.text = $"第{QuadrantManager.Instance.CurrentQuadrant}象限";

        if (starSystemText != null && StarSystemManager.Instance != null)
        {
            string sysName = StarSystemManager.Instance.GetStarSystemName(pos);
            starSystemText.text = string.IsNullOrEmpty(sysName) ? "無星域" : $"[Γ]{sysName}星域";
        }
    }
}
