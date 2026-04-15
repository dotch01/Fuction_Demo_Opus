using UnityEngine;
using System.Collections.Generic;

// ============================================================
// MultiReflectionExhibit.cs — 反射碰撞
// 球碰牆反彈多次，軌跡全顯示
// ============================================================

public class MultiReflectionExhibit : ExhibitBase
{
    private DragHandle handleDir;
    private TextMesh bounceLabel, infoLabel;
    private int maxBounces = 8;

    public override void BuildExhibit()
    {
        exhibitName = "多次反射 Multi-Reflection";
        description = "光線/球射出碰牆反彈：\n\nR = D - 2(D·N)N\n\n每次碰牆用反射公式算新方向\n重複直到能量耗盡或達彈跳上限\n\n🎮 遊戲應用：\n• 彈珠台/Breakout 的球反彈\n• 射擊遊戲子彈跳彈（Ricochet）\n• 光線追蹤：多次反射的鏡面效果\n• 聲音傳播模擬（回音）\n\n拖曳改變射出方向";
        formula = "R = D - 2(D·N)N";
        challengeDescription = "讓光線反彈 5 次以上";

        handleDir = CreateDragHandle(new Vector3(0, 1f, 0), new Color(1f, 0.85f, 0.2f), 0.12f);

        bounceLabel = CreateLabel(new Vector3(0, -2.2f, 0), "", 30, Color.white);
        infoLabel = CreateLabel(new Vector3(0, -2.8f, 0), "", 24, new Color(0.7f, 0.7f, 0.8f));
    }

    public override void UpdateVisualization()
    {
        var mr = MathLineRenderer.Instance;
        if (mr == null) return;

        // 房間邊界 (local)
        float hW = 2.5f, hH = 2f;
        Vector3 bl = new Vector3(-hW, -hH, 0), br = new Vector3(hW, -hH, 0);
        Vector3 tl = new Vector3(-hW, hH, 0), tr = new Vector3(hW, hH, 0);

        mr.DrawLine(transform.TransformPoint(bl), transform.TransformPoint(br), new Color(0.4f, 0.4f, 0.5f), 0.008f);
        mr.DrawLine(transform.TransformPoint(br), transform.TransformPoint(tr), new Color(0.4f, 0.4f, 0.5f), 0.008f);
        mr.DrawLine(transform.TransformPoint(tr), transform.TransformPoint(tl), new Color(0.4f, 0.4f, 0.5f), 0.008f);
        mr.DrawLine(transform.TransformPoint(tl), transform.TransformPoint(bl), new Color(0.4f, 0.4f, 0.5f), 0.008f);

        // 射線追蹤
        Vector3 origin = new Vector3(-hW + 0.1f, 0, 0);
        Vector3 dir = (handleDir.LocalPosition - origin).normalized;

        int bounces = 0;
        Vector3 pos = origin;

        for (int i = 0; i < maxBounces; i++)
        {
            // 找最近的牆壁交點
            float minT = float.MaxValue;
            Vector3 hitNormal = Vector3.zero;

            TryWallHit(pos, dir, -hW, true, false, ref minT, ref hitNormal); // left
            TryWallHit(pos, dir, hW, true, true, ref minT, ref hitNormal);   // right
            TryWallHit(pos, dir, -hH, false, false, ref minT, ref hitNormal); // bottom
            TryWallHit(pos, dir, hH, false, true, ref minT, ref hitNormal);   // top

            if (minT > 100f) break;

            Vector3 hitPos = pos + dir * minT;
            float hue = (float)i / maxBounces;
            Color c = Color.HSVToRGB(hue * 0.8f, 0.9f, 1f);
            mr.DrawLine(transform.TransformPoint(pos), transform.TransformPoint(hitPos), c, 0.012f);

            bounces++;
            pos = hitPos + hitNormal * 0.01f;
            dir = dir - 2f * Vector3.Dot(dir, hitNormal) * hitNormal;
        }

        bounceLabel.text = $"反彈次數: {bounces}";
        bounceLabel.color = bounces >= 5 ? new Color(0.3f, 1f, 0.5f) : Color.white;
        infoLabel.text = "R = D - 2(D·N)N    每次反彈顏色漸變";
    }

    private void TryWallHit(Vector3 pos, Vector3 dir, float wallVal, bool isVertical, bool isPositive, ref float minT, ref Vector3 hitNormal)
    {
        float t;
        if (isVertical) // x = wallVal
        {
            if (Mathf.Abs(dir.x) < 0.0001f) return;
            t = (wallVal - pos.x) / dir.x;
        }
        else // y = wallVal
        {
            if (Mathf.Abs(dir.y) < 0.0001f) return;
            t = (wallVal - pos.y) / dir.y;
        }

        if (t > 0.001f && t < minT)
        {
            Vector3 hit = pos + dir * t;
            float hW = 2.5f, hH = 2f;
            if (isVertical && Mathf.Abs(hit.y) <= hH + 0.01f)
            {
                minT = t;
                hitNormal = isPositive ? Vector3.left : Vector3.right;
            }
            else if (!isVertical && Mathf.Abs(hit.x) <= hW + 0.01f)
            {
                minT = t;
                hitNormal = isPositive ? Vector3.down : Vector3.up;
            }
        }
    }

    public override bool CheckChallengeComplete()
    {
        // 簡化：計算彈跳次數
        float hW = 2.5f, hH = 2f;
        Vector3 origin = new Vector3(-hW + 0.1f, 0, 0);
        Vector3 dir = (handleDir.LocalPosition - origin).normalized;
        Vector3 pos = origin;
        int count = 0;
        for (int i = 0; i < maxBounces; i++)
        {
            float minT = float.MaxValue;
            Vector3 normal = Vector3.zero;
            TryWallHit(pos, dir, -hW, true, false, ref minT, ref normal);
            TryWallHit(pos, dir, hW, true, true, ref minT, ref normal);
            TryWallHit(pos, dir, -hH, false, false, ref minT, ref normal);
            TryWallHit(pos, dir, hH, false, true, ref minT, ref normal);
            if (minT > 100f) break;
            pos = pos + dir * minT + normal * 0.01f;
            dir = dir - 2f * Vector3.Dot(dir, normal) * normal;
            count++;
        }
        return count >= 5;
    }
}
