using UnityEngine;

// ============================================================
// ComplexMultiplyExhibit.cs — 複數旋轉
// 兩個複數相乘 = 旋轉 + 縮放
// ============================================================

public class ComplexMultiplyExhibit : ExhibitBase
{
    private DragHandle handleA, handleB;
    private TextMesh resultLabel, polarLabel, infoLabel;

    public override void BuildExhibit()
    {
        exhibitName = "複數旋轉 Complex Multiply";
        description = "複數 z = a + bi 可以表示 2D 旋轉！\n\n兩複數相乘：\n(a+bi)(c+di) = (ac-bd) + (ad+bc)i\n\n效果 = 角度相加 + 長度相乘\n\n🎮 遊戲應用：\n• 2D 旋轉不需要矩陣！用複數乘法即可\n• Quaternion 是「4D 複數」的延伸\n• 訊號處理中的相位旋轉\n\n拖曳兩個複數觀察乘積";
        formula = "z₁·z₂ = |z₁|·|z₂| ∠(θ₁+θ₂)";
        challengeDescription = "讓乘積的角度超過 270°";

        handleA = CreateDragHandle(new Vector3(1f, 1f, 0), new Color(0.3f, 0.8f, 1f), 0.12f);
        handleB = CreateDragHandle(new Vector3(-0.5f, 1f, 0), new Color(1f, 0.5f, 0.3f), 0.12f);

        resultLabel = CreateLabel(new Vector3(0, -1.8f, 0), "", 28, Color.white);
        polarLabel = CreateLabel(new Vector3(0, -2.4f, 0), "", 26, new Color(0.7f, 0.8f, 1f));
        infoLabel = CreateLabel(new Vector3(0, -3f, 0), "", 24, new Color(0.7f, 0.7f, 0.8f));
    }

    public override void UpdateVisualization()
    {
        var mr = MathLineRenderer.Instance;
        if (mr == null) return;

        Vector3 origin = transform.position;
        Vector2 a = new Vector2(handleA.LocalPosition.x, handleA.LocalPosition.y);
        Vector2 b = new Vector2(handleB.LocalPosition.x, handleB.LocalPosition.y);

        // 複數乘法
        Vector2 product = new Vector2(a.x * b.x - a.y * b.y, a.x * b.y + a.y * b.x);

        // 畫三個向量
        mr.DrawArrow(origin, transform.TransformPoint(new Vector3(a.x, a.y, 0)),
            new Color(0.3f, 0.8f, 1f), 0.012f, 0.06f);
        mr.DrawArrow(origin, transform.TransformPoint(new Vector3(b.x, b.y, 0)),
            new Color(1f, 0.5f, 0.3f), 0.012f, 0.06f);
        mr.DrawArrow(origin, transform.TransformPoint(new Vector3(product.x, product.y, 0)),
            new Color(0.3f, 1f, 0.5f), 0.018f, 0.08f);

        // 畫單位圓參考
        mr.DrawCircle(origin, 1f, transform.forward, new Color(0.3f, 0.3f, 0.35f), 0.005f, 36);

        float angleA = Mathf.Atan2(a.y, a.x) * Mathf.Rad2Deg;
        float angleB = Mathf.Atan2(b.y, b.x) * Mathf.Rad2Deg;
        float angleP = Mathf.Atan2(product.y, product.x) * Mathf.Rad2Deg;
        float magA = a.magnitude, magB = b.magnitude, magP = product.magnitude;

        resultLabel.text = $"({a.x:F1}+{a.y:F1}i) × ({b.x:F1}+{b.y:F1}i) = ({product.x:F1}+{product.y:F1}i)";
        polarLabel.text = $"∠{angleA:F0}° + ∠{angleB:F0}° = ∠{angleP:F0}°　|{magA:F1}|×|{magB:F1}| = |{magP:F1}|";
        infoLabel.text = "複數乘法 = 角度加 + 長度乘";
    }

    public override bool CheckChallengeComplete()
    {
        Vector2 a = new Vector2(handleA.LocalPosition.x, handleA.LocalPosition.y);
        Vector2 b = new Vector2(handleB.LocalPosition.x, handleB.LocalPosition.y);
        float angleA = Mathf.Atan2(a.y, a.x) * Mathf.Rad2Deg;
        float angleB = Mathf.Atan2(b.y, b.x) * Mathf.Rad2Deg;
        float total = angleA + angleB;
        if (total < 0) total += 360f;
        return total > 270f;
    }
}
