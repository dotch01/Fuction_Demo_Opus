using UnityEngine;

/// <summary>
/// 象限管理器：根據鏡頭位置判斷目前象限，跨象限時 Log。
/// 象限排列:
///   Q1(左上) Q2(右上)
///   Q3(左下) Q4(右下)
/// </summary>
public class QuadrantManager : MonoBehaviour
{
    public static QuadrantManager Instance { get; private set; }

    [Header("世界設定")]
    [SerializeField] private float worldWidth = 100f;
    [SerializeField] private float worldHeight = 100f;

    public int CurrentQuadrant { get; private set; } = -1;
    public Vector2 CameraPosition { get; private set; }

    private float HalfWidth => worldWidth * 0.5f;
    private float HalfHeight => worldHeight * 0.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        Vector3 camPos = Camera.main.transform.position;
        // 鏡頭自由移動不受限，但邏輯座標始終包在 [0, worldSize) 內
        CameraPosition = new Vector2(
            Mathf.Repeat(camPos.x, worldWidth),
            Mathf.Repeat(camPos.y, worldHeight));

        int newQuadrant = GetQuadrant(CameraPosition);
        if (newQuadrant != CurrentQuadrant)
        {
            CurrentQuadrant = newQuadrant;
            Debug.Log($"進入第{CurrentQuadrant}象限, 位置: ({CameraPosition.x:F2}, {CameraPosition.y:F2})");
        }
    }

    /// <summary>
    /// 根據座標回傳象限編號 (1~4)。
    /// Q1(左上) Q2(右上) Q3(左下) Q4(右下)
    /// </summary>
    public int GetQuadrant(Vector2 pos)
    {
        bool isRight = pos.x >= HalfWidth;
        bool isTop = pos.y >= HalfHeight;

        if (!isRight && isTop) return 1;
        if (isRight && isTop) return 2;
        if (!isRight && !isTop) return 3;
        return 4; // isRight && !isTop
    }
}
