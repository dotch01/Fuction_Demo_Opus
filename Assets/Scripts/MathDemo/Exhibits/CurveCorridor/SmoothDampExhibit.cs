using UnityEngine;

// ============================================================
// SmoothDampExhibit.cs — 平滑移動對比
// Lerp vs SmoothDamp 追蹤目標
// ============================================================

public class SmoothDampExhibit : ExhibitBase
{
    private DragHandle handleTarget;
    private GameObject ballLerp, ballSmooth;
    private TextMesh lerpLabel, smoothLabel, infoLabel;

    private Vector3 lerpPos, smoothPos;
    private Vector3 smoothVelocity = Vector3.zero;
    private float lerpSpeed = 3f;
    private float smoothTime = 0.3f;

    public override void BuildExhibit()
    {
        exhibitName = "平滑移動 SmoothDamp";
        description = "兩種追蹤目標的方式：\n\n• Lerp（白球）：pos = Lerp(pos, target, speed×dt)\n  簡單但會「震盪」或永遠到不了\n\n• SmoothDamp（綠球）：帶速度的彈簧阻尼\n  自然加減速，保證到達\n\n🎮 遊戲應用：\n• 第三人稱相機跟隨：SmoothDamp 最自然\n• UI 浮動元素平滑移動\n• 鏡頭震動後的回彈恢復\n\n拖曳目標（黃色）觀察差異";
        formula = "SmoothDamp: 批判阻尼彈簧\nLerp: pos += (target-pos) × t";
        challengeDescription = "快速拖曳目標讓 Lerp 和 SmoothDamp 明顯分離";

        handleTarget = CreateDragHandle(new Vector3(0, 0, 0), new Color(1f, 0.85f, 0.2f), 0.15f);

        ballLerp = CreateStaticPrimitive(PrimitiveType.Sphere, new Vector3(-1f, 0, 0), Vector3.one * 0.2f, Color.white);
        ballSmooth = CreateStaticPrimitive(PrimitiveType.Sphere, new Vector3(1f, 0, 0), Vector3.one * 0.2f, new Color(0.3f, 1f, 0.5f));

        lerpPos = new Vector3(-1f, 0, 0);
        smoothPos = new Vector3(1f, 0, 0);

        lerpLabel = CreateLabel(new Vector3(-2.5f, -1.5f, 0), "Lerp", 26, Color.white);
        smoothLabel = CreateLabel(new Vector3(2.5f, -1.5f, 0), "SmoothDamp", 26, new Color(0.3f, 1f, 0.5f));
        infoLabel = CreateLabel(new Vector3(0, -2.2f, 0), "", 24, new Color(0.7f, 0.7f, 0.8f));
    }

    public override void UpdateVisualization()
    {
        var mr = MathLineRenderer.Instance;
        if (mr == null) return;

        Vector3 target = handleTarget.LocalPosition;

        // Lerp
        lerpPos = Vector3.Lerp(lerpPos, target, lerpSpeed * Time.deltaTime);
        ballLerp.transform.localPosition = lerpPos;

        // SmoothDamp
        smoothPos = Vector3.SmoothDamp(smoothPos, target, ref smoothVelocity, smoothTime);
        ballSmooth.transform.localPosition = smoothPos;

        // 畫到目標的線
        mr.DrawLine(transform.TransformPoint(lerpPos), transform.TransformPoint(target), new Color(0.6f, 0.6f, 0.6f, 0.4f), 0.005f);
        mr.DrawLine(transform.TransformPoint(smoothPos), transform.TransformPoint(target), new Color(0.3f, 0.8f, 0.4f, 0.4f), 0.005f);

        float distL = Vector3.Distance(lerpPos, target);
        float distS = Vector3.Distance(smoothPos, target);
        infoLabel.text = $"Lerp 距離: {distL:F3}    SmoothDamp 距離: {distS:F3}";
    }

    public override bool CheckChallengeComplete()
    {
        return Vector3.Distance(lerpPos, smoothPos) > 0.8f;
    }
}
