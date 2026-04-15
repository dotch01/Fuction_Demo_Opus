using UnityEngine;

// ============================================================
// LSystemExhibit.cs — L-system 植物
// 規則重寫 + 烏龜繪圖
// ============================================================

public class LSystemExhibit : ExhibitBase
{
    private DragHandle handleIter, handleAngle;
    private TextMesh iterLabel, ruleLabel;

    public override void BuildExhibit()
    {
        exhibitName = "L-System 植物";
        description = "Lindenmayer System — 語法生成植物：\n\n規則：F → FF+[+F-F-F]-[-F+F+F]\n\nF = 前進畫線\n+/- = 左/右轉  [ ] = 存/取狀態\n\n🎮 遊戲應用：\n• 程序化生成樹木/灌木/花朵\n• 珊瑚/根系等自然結構\n• 城市道路網絡生成\n• 用語法規則產生無限變化\n\n拖曳控制迭代次數和分支角度";
        formula = "F → FF+[+F-F-F]-[-F+F+F]";
        challengeDescription = "讓迭代到 Level 3 + 角度 > 20°";

        handleIter = CreateDragHandle(new Vector3(-2.5f, -2f, 0), new Color(0.3f, 0.8f, 1f), 0.12f);
        handleIter.minBounds = new Vector3(-2.5f, -2.5f, 0);
        handleIter.maxBounds = new Vector3(-2.5f, -0.5f, 0);

        handleAngle = CreateDragHandle(new Vector3(2.5f, -2f, 0), new Color(0.5f, 1f, 0.4f), 0.12f);
        handleAngle.minBounds = new Vector3(2.5f, -2.5f, 0);
        handleAngle.maxBounds = new Vector3(2.5f, -0.5f, 0);

        CreateLabel(new Vector3(-2.5f, -3f, 0), "迭代", 20, new Color(0.5f, 0.5f, 0.6f));
        CreateLabel(new Vector3(2.5f, -3f, 0), "角度", 20, new Color(0.5f, 0.5f, 0.6f));

        iterLabel = CreateLabel(new Vector3(0, -2.5f, 0), "", 28, Color.white);
        ruleLabel = CreateLabel(new Vector3(0, -3.2f, 0), "F → F[+F]F[-F]F", 22, new Color(0.7f, 0.7f, 0.8f));
    }

    public override void UpdateVisualization()
    {
        var mr = MathLineRenderer.Instance;
        if (mr == null) return;

        int iter = Mathf.RoundToInt(Mathf.Lerp(1, 4, Mathf.InverseLerp(-2.5f, -0.5f, handleIter.LocalPosition.y)));
        float angle = Mathf.Lerp(10f, 40f, Mathf.InverseLerp(-2.5f, -0.5f, handleAngle.LocalPosition.y));

        // 生成 L-system 字串
        string axiom = "F";
        string current = axiom;
        for (int i = 0; i < iter; i++)
        {
            var sb = new System.Text.StringBuilder();
            foreach (char c in current)
            {
                if (c == 'F') sb.Append("F[+F]F[-F]F");
                else sb.Append(c);
            }
            current = sb.ToString();
            if (current.Length > 5000) break; // 安全限制
        }

        // 烏龜繪圖
        float stepLen = 0.15f / Mathf.Pow(2, iter - 1);
        Vector3 pos = new Vector3(0, -1.5f, 0);
        float dir = 90f; // 向上

        var stack = new System.Collections.Generic.Stack<(Vector3, float)>();
        int lineCount = 0;

        foreach (char c in current)
        {
            if (lineCount > 2000) break;

            switch (c)
            {
                case 'F':
                    Vector3 newPos = pos + new Vector3(
                        Mathf.Cos(dir * Mathf.Deg2Rad) * stepLen,
                        Mathf.Sin(dir * Mathf.Deg2Rad) * stepLen, 0);
                    float hue = Mathf.Clamp01((pos.y + 1.5f) / 3f) * 0.3f;
                    Color col = Color.HSVToRGB(hue + 0.2f, 0.7f, 0.8f);
                    mr.DrawLine(transform.TransformPoint(pos), transform.TransformPoint(newPos), col, 0.005f);
                    pos = newPos;
                    lineCount++;
                    break;
                case '+': dir += angle; break;
                case '-': dir -= angle; break;
                case '[': stack.Push((pos, dir)); break;
                case ']':
                    if (stack.Count > 0)
                    {
                        var state = stack.Pop();
                        pos = state.Item1;
                        dir = state.Item2;
                    }
                    break;
            }
        }

        iterLabel.text = $"Level {iter}    角度 {angle:F0}°    線段 {lineCount}";
    }

    public override bool CheckChallengeComplete()
    {
        int iter = Mathf.RoundToInt(Mathf.Lerp(1, 4, Mathf.InverseLerp(-2.5f, -0.5f, handleIter.LocalPosition.y)));
        float angle = Mathf.Lerp(10f, 40f, Mathf.InverseLerp(-2.5f, -0.5f, handleAngle.LocalPosition.y));
        return iter >= 3 && angle > 20f;
    }
}
