using UnityEngine;

// ============================================================
// EasingExhibit.cs — 動畫緩動函數
// 多個球使用不同 easing 同時起跑，比較速度曲線
// ============================================================

public class EasingExhibit : ExhibitBase
{
    private GameObject[] balls;
    private TextMesh[] nameLabels;
    private float animTime = 0;
    private bool animPlaying = true;
    private TextMesh timerLabel;

    private readonly string[] easingNames = {
        "Linear", "EaseInQuad", "EaseOutQuad", "EaseInOutCubic",
        "EaseOutBounce", "EaseOutElastic", "EaseInExpo", "EaseOutBack"
    };

    public override void BuildExhibit()
    {
        exhibitName = "Easing 動畫函數";
        description = "動畫不是只有線性移動！\n\nEasing 函數讓動畫有加速、減速、彈跳等感覺\n• EaseIn = 慢開始\n• EaseOut = 慢結束\n• Bounce = 彈跳\n• Elastic = 彈性\n\n🎮 遊戲應用：\n• UI 彈出動畫：EaseOutBack 有彈性感\n• 場景轉換：EaseInOut 自然過渡\n• 角色跳躍弧線：非線性高度變化\n• 掉落物彈跳：Bounce 效果\n\n觀察 8 個球同時起跑的差異";
        formula = "easeOutBounce, easeInQuad, easeOutElastic...";
        challengeDescription = "按 E 重播動畫觀察差異";

        balls = new GameObject[easingNames.Length];
        nameLabels = new TextMesh[easingNames.Length];

        for (int i = 0; i < easingNames.Length; i++)
        {
            float y = 2.5f - i * 0.75f;
            Color c = Color.HSVToRGB((float)i / easingNames.Length, 0.7f, 0.9f);

            balls[i] = CreateStaticPrimitive(PrimitiveType.Sphere,
                new Vector3(-3.5f, y, 0), Vector3.one * 0.25f, c);

            nameLabels[i] = CreateLabel(new Vector3(-3.5f, y + 0.3f, 0), easingNames[i], 20, c);
        }

        timerLabel = CreateLabel(new Vector3(0, -3.5f, 0), "按 E 重播", 30, Color.white);
    }

    public override void UpdateVisualization()
    {
        if (MathLineRenderer.Instance == null || balls == null) return;
        var mr = MathLineRenderer.Instance;

        if (animPlaying)
        {
            animTime += Time.deltaTime * 0.4f;
            if (animTime > 1.3f) { animTime = 1.3f; animPlaying = false; }
        }

        float t = Mathf.Clamp01(animTime);

        for (int i = 0; i < balls.Length; i++)
        {
            float y = 2.5f - i * 0.75f;
            float easedT = ApplyEasing(i, t);
            float x = Mathf.Lerp(-3.5f, 3.5f, easedT);
            balls[i].transform.localPosition = new Vector3(x, y, 0);

            // 軌跡（起點到終點的線）
            mr.DrawLine(transform.TransformPoint(new Vector3(-3.5f, y, 0)),
                        transform.TransformPoint(new Vector3(3.5f, y, 0)),
                        new Color(0.25f, 0.25f, 0.3f), 0.005f);
        }

        timerLabel.text = animPlaying ? $"t = {t:F2}" : "完成！按 E 重播";
    }

    protected override void OnChallengeStart()
    {
        // 重播動畫
        animTime = 0;
        animPlaying = true;
        challengeCompleted = true; // 觀看即完成
        ChallengeSystem.Instance?.MarkCompleted(exhibitName);
    }

    private float ApplyEasing(int index, float t)
    {
        switch (index)
        {
            case 0: return t; // linear
            case 1: return t * t; // easeInQuad
            case 2: return 1f - (1f - t) * (1f - t); // easeOutQuad
            case 3: return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f; // easeInOutCubic
            case 4: return EaseOutBounce(t);
            case 5: return EaseOutElastic(t);
            case 6: return t == 0 ? 0 : Mathf.Pow(2f, 10f * t - 10f); // easeInExpo
            case 7: // easeOutBack
                float c1 = 1.70158f;
                float c3 = c1 + 1f;
                return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
            default: return t;
        }
    }

    private float EaseOutBounce(float t)
    {
        float n1 = 7.5625f, d1 = 2.75f;
        if (t < 1f / d1) return n1 * t * t;
        if (t < 2f / d1) { t -= 1.5f / d1; return n1 * t * t + 0.75f; }
        if (t < 2.5f / d1) { t -= 2.25f / d1; return n1 * t * t + 0.9375f; }
        t -= 2.625f / d1; return n1 * t * t + 0.984375f;
    }

    private float EaseOutElastic(float t)
    {
        if (t <= 0) return 0;
        if (t >= 1) return 1;
        float c4 = (2f * Mathf.PI) / 3f;
        return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
    }

    public override bool CheckChallengeComplete() => challengeCompleted;
}
