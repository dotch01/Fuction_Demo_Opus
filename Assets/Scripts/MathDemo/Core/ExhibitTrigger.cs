using UnityEngine;

// ============================================================
// ExhibitTrigger.cs
// 接近展品時觸發啟用/停用，管理 UI 顯示
// ============================================================

[RequireComponent(typeof(BoxCollider))]
public class ExhibitTrigger : MonoBehaviour
{
    public ExhibitBase exhibit;
    public float triggerRadius = 5f;

    private bool playerInside = false;

    void Start()
    {
        // 設定觸發區域
        var col = GetComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = Vector3.one * triggerRadius * 2f;
        col.center = Vector3.zero;

        // 確保有 Rigidbody（觸發需要）
        if (GetComponent<Rigidbody>() == null)
        {
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (exhibit == null) return;

        playerInside = true;
        exhibit.Activate();

        // 通知 UI
        var ui = FindAnyObjectByType<ExhibitInfoPanel>();
        if (ui != null) ui.Show(exhibit);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (exhibit == null) return;

        playerInside = false;
        exhibit.Deactivate();

        var ui = FindAnyObjectByType<ExhibitInfoPanel>();
        if (ui != null) ui.Hide();
    }

    public bool IsPlayerInside => playerInside;
}
