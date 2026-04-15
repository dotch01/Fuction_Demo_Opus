using UnityEngine;
using UnityEngine.UI;

// ============================================================
// ChallengeHUD.cs
// 右上角進度顯示 + 挑戰成功通知
// ============================================================

public class ChallengeHUD : MonoBehaviour
{
    private Text progressText;
    private Text notificationText;
    private float notificationTimer;

    public void Initialize(Transform canvasTransform, Font font)
    {
        // 進度
        var progressGo = new GameObject("ChallengeProgress");
        progressGo.transform.SetParent(canvasTransform, false);
        var pRt = progressGo.AddComponent<RectTransform>();
        pRt.anchorMin = new Vector2(0.82f, 0.93f);
        pRt.anchorMax = new Vector2(0.99f, 0.99f);
        pRt.offsetMin = Vector2.zero;
        pRt.offsetMax = Vector2.zero;

        progressText = progressGo.AddComponent<Text>();
        progressText.font = font;
        progressText.fontSize = 18;
        progressText.color = new Color(0.7f, 0.8f, 0.9f);
        progressText.alignment = TextAnchor.MiddleRight;
        progressText.text = "★ 0 / 0";
        progressText.raycastTarget = false;

        var shadow = progressGo.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.8f);

        // 通知（中央上方）
        var notifGo = new GameObject("ChallengeNotification");
        notifGo.transform.SetParent(canvasTransform, false);
        var nRt = notifGo.AddComponent<RectTransform>();
        nRt.anchorMin = new Vector2(0.25f, 0.75f);
        nRt.anchorMax = new Vector2(0.75f, 0.85f);
        nRt.offsetMin = Vector2.zero;
        nRt.offsetMax = Vector2.zero;

        notificationText = notifGo.AddComponent<Text>();
        notificationText.font = font;
        notificationText.fontSize = 28;
        notificationText.color = new Color(1f, 0.9f, 0.3f, 0f);
        notificationText.alignment = TextAnchor.MiddleCenter;
        notificationText.raycastTarget = false;

        var nShadow = notifGo.AddComponent<Shadow>();
        nShadow.effectColor = new Color(0, 0, 0, 0.8f);

        // 訂閱事件
        if (ChallengeSystem.Instance != null)
        {
            ChallengeSystem.Instance.onChallengeCompleted += OnChallengeCompleted;
        }
    }

    void Update()
    {
        // 更新進度
        if (progressText != null && ChallengeSystem.Instance != null)
        {
            var cs = ChallengeSystem.Instance;
            progressText.text = $"★ {cs.CompletedCount} / {cs.TotalExhibits}";
        }

        // 通知淡出
        if (notificationTimer > 0)
        {
            notificationTimer -= Time.deltaTime;
            float alpha = Mathf.Clamp01(notificationTimer / 0.5f);
            if (notificationText != null)
                notificationText.color = new Color(1f, 0.9f, 0.3f, alpha);
        }
    }

    private void OnChallengeCompleted(string exhibitName, int completed, int total)
    {
        if (notificationText != null)
        {
            notificationText.text = $"★ {exhibitName} 挑戰完成！";
            notificationText.color = new Color(1f, 0.9f, 0.3f, 1f);
            notificationTimer = 3f;
        }
    }
}
