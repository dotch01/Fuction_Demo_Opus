using UnityEngine;

// ============================================================
// BillboardExhibit.cs — Billboarding（面對攝影機）
// 多個物件，切換不同 Billboard 模式
// ============================================================

public class BillboardExhibit : ExhibitBase
{
    private GameObject[] billboards;
    private TextMesh[] modeLabels;
    private TextMesh infoLabel;
    private int currentMode = 0; // 0=Off, 1=World, 2=Axial

    private readonly string[] modeNames = { "關閉 Off", "完全朝向 World", "軸向 Axial (Y-only)" };

    public override void BuildExhibit()
    {
        exhibitName = "Billboarding 面向攝影機";
        description = "讓 2D 物件永遠面向鏡頭！\n\n• World Billboard = 完全朝向相機\n  → 適合粒子、光暈、爆炸特效\n• Axial Billboard = 只繞 Y 軸旋轉\n  → 適合樹木、NPC 名牌\n\n🎮 遊戲應用：\n• 粒子系統的每個粒子都是 Billboard\n• 遠處樹木用 2D 圖片代替 3D 模型\n• 血條/名字牌永遠面向玩家\n• 傷害數字浮動效果\n\n按 E 切換模式，走動觀察差異";
        formula = "forward = normalize(camera.pos - obj.pos)\nobj.rotation = LookRotation(forward)";
        challengeDescription = "切換到 Axial 模式觀察差異";

        billboards = new GameObject[5];
        modeLabels = new TextMesh[5];

        Color[] colors = {
            new Color(1f, 0.4f, 0.4f), new Color(0.4f, 0.9f, 0.4f),
            new Color(0.4f, 0.4f, 1f), new Color(1f, 0.9f, 0.3f),
            new Color(0.9f, 0.4f, 0.9f)
        };

        for (int i = 0; i < 5; i++)
        {
            float x = (i - 2) * 1.8f;
            var quad = CreateStaticPrimitive(PrimitiveType.Quad,
                new Vector3(x, 0.5f, 0), Vector3.one * 0.9f, colors[i]);

            billboards[i] = quad;
            modeLabels[i] = CreateLabel(new Vector3(x, -0.5f, 0), $"#{i + 1}", 22, colors[i]);
        }

        infoLabel = CreateLabel(new Vector3(0, -2.5f, 0), "", 34, Color.white);
    }

    public override void UpdateVisualization()
    {
        if (billboards == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        for (int i = 0; i < billboards.Length; i++)
        {
            if (billboards[i] == null) continue;

            switch (currentMode)
            {
                case 0: // Off — 不旋轉
                    billboards[i].transform.localRotation = Quaternion.identity;
                    break;

                case 1: // World Billboard — 完全朝向相機
                    Vector3 toCamera = cam.transform.position - billboards[i].transform.position;
                    if (toCamera.sqrMagnitude > 0.001f)
                        billboards[i].transform.rotation = Quaternion.LookRotation(-toCamera);
                    break;

                case 2: // Axial Billboard — 只繞 Y 軸
                    Vector3 toCamFlat = cam.transform.position - billboards[i].transform.position;
                    toCamFlat.y = 0;
                    if (toCamFlat.sqrMagnitude > 0.001f)
                        billboards[i].transform.rotation = Quaternion.LookRotation(-toCamFlat);
                    break;
            }
        }

        infoLabel.text = $"模式：{modeNames[currentMode]}    按 E 切換";

        // 畫向量到相機
        if (MathLineRenderer.Instance != null && currentMode > 0)
        {
            var mr = MathLineRenderer.Instance;
            for (int i = 0; i < billboards.Length; i++)
            {
                Vector3 dir = (cam.transform.position - billboards[i].transform.position).normalized;
                mr.DrawArrow(billboards[i].transform.position,
                             billboards[i].transform.position + dir * 0.6f,
                             new Color(1f, 1f, 1f, 0.3f), 0.01f);
            }
        }
    }

    protected override void OnChallengeStart()
    {
        currentMode = (currentMode + 1) % 3;
        if (currentMode == 2)
        {
            challengeCompleted = true;
            ChallengeSystem.Instance?.MarkCompleted(exhibitName);
        }
    }

    public override bool CheckChallengeComplete() => challengeCompleted;
}
