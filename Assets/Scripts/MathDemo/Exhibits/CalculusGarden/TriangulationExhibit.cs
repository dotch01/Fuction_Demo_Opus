using UnityEngine;

// ============================================================
// TriangulationExhibit.cs — 三角測量定位
// 三個信號塔 + 三個距離圓 → 交點定位
// ============================================================

public class TriangulationExhibit : ExhibitBase
{
    private DragHandle handleTarget;
    private TextMesh resultLabel, infoLabel;
    private Vector3[] towers = { new Vector3(-1.5f, 1f, 0), new Vector3(1.5f, 1f, 0), new Vector3(0, -1.2f, 0) };
    private Color[] towerColors = { new Color(0.3f, 0.8f, 1f), new Color(1f, 0.5f, 0.3f), new Color(0.5f, 1f, 0.4f) };

    public override void BuildExhibit()
    {
        exhibitName = "三角測量 Triangulation";
        description = "GPS / WiFi 定位原理：\n\n知道到 3 個基站的距離\n→ 三個圓的交點 = 你的位置！\n\n🎮 遊戲應用：\n• 手遊 AR 的室內定位\n• 遊戲中的雷達/聲納定位機制\n• 多人遊戲的延遲三角測量\n• 敵人位置推測（聲音方向+距離）\n\n拖曳目標點觀察三圓交匯定位";
        formula = "(x-xᵢ)²+(y-yᵢ)² = dᵢ²";
        challengeDescription = "把目標拖到三圓交匯處";

        for (int i = 0; i < 3; i++)
        {
            CreateStaticPrimitive(PrimitiveType.Cube, towers[i], Vector3.one * 0.15f, towerColors[i]);
            CreateLabel(towers[i] + Vector3.up * 0.3f, $"T{i + 1}", 20, towerColors[i]);
        }

        handleTarget = CreateDragHandle(new Vector3(0.3f, 0.2f, 0), new Color(1f, 0.85f, 0.2f));

        resultLabel = CreateLabel(new Vector3(0, -2.3f, 0), "", 28, Color.white);
        infoLabel = CreateLabel(new Vector3(0, -2.9f, 0), "", 24, new Color(0.7f, 0.7f, 0.8f));
    }

    public override void UpdateVisualization()
    {
        var mr = MathLineRenderer.Instance;
        if (mr == null) return;

        Vector3 target = handleTarget.LocalPosition;

        for (int i = 0; i < 3; i++)
        {
            float dist = Vector3.Distance(target, towers[i]);

            // 距離圓
            mr.DrawCircle(transform.TransformPoint(towers[i]), dist, transform.forward, towerColors[i] * 0.7f, 0.008f, 36);

            // 到目標的連線
            mr.DrawLine(transform.TransformPoint(towers[i]), transform.TransformPoint(target),
                towerColors[i] * 0.5f, 0.005f);
        }

        float d1 = Vector3.Distance(target, towers[0]);
        float d2 = Vector3.Distance(target, towers[1]);
        float d3 = Vector3.Distance(target, towers[2]);

        resultLabel.text = $"d₁={d1:F2}  d₂={d2:F2}  d₃={d3:F2}";
        infoLabel.text = $"目標 ({target.x:F2}, {target.y:F2}) — 三圓交匯定位";
    }

    public override bool CheckChallengeComplete()
    {
        // 三圓交匯 = 三圓都通過目標點（自動滿足因為半徑就是距離）
        return true; // 這個展品只要觀看就算完成
    }

    protected override void OnChallengeStart()
    {
        challengeCompleted = true;
        ChallengeSystem.Instance?.MarkCompleted(exhibitName);
    }
}
