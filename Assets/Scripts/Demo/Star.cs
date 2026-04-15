using UnityEngine;

/// <summary>
/// 星星資料元件，掛在每顆星星 GameObject 上。
/// 持有 PlanetEntry 參照，支援掃描後命名 + 玩家改名。
/// </summary>
public class Star : MonoBehaviour
{
    public PlanetEntry Data { get; private set; }
    public string StarName { get; private set; }
    public bool IsScanned { get; private set; }
    public bool FilterOnly => Data != null && Data.filterOnly;

    /// <summary>建立時的原始世界座標 (0~worldSize)，用於 wrap 計算。</summary>
    public Vector2 OriginalPosition { get; private set; }

    private TextMesh _label;

    public void Init(PlanetEntry data)
    {
        Data = data;
        OriginalPosition = new Vector2(transform.position.x, transform.position.y);
    }

    public void Scan(string assignedName)
    {
        StarName = assignedName;
        IsScanned = true;
        gameObject.name = assignedName;
        UpdateLabel(assignedName);
    }

    public void Rename(string newName)
    {
        StarName = newName;
        gameObject.name = newName;
        UpdateLabel(newName);
    }

    private void UpdateLabel(string text)
    {
        if (_label == null)
        {
            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(transform, false);
            labelGo.transform.localPosition = new Vector3(0, 1.2f, 0);
            labelGo.transform.localScale = Vector3.one * (3f / Mathf.Max(transform.localScale.x, 0.1f));

            _label = labelGo.AddComponent<TextMesh>();
            _label.characterSize = 0.15f;
            _label.fontSize = 48;
            _label.anchor = TextAnchor.LowerCenter;
            _label.alignment = TextAlignment.Center;
            _label.color = Color.white;

            var mr = labelGo.GetComponent<MeshRenderer>();
            mr.sortingOrder = 5;
        }

        _label.text = text;
    }
}
