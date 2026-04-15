using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 正交鏡頭控制器：WASD/方向鍵平移 + 滑鼠虛擬搖桿(方向線) + 彈性跟隨 + 世界邊界回繞。
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 15f;
    [SerializeField] private float dragMaxDistance = 200f;

    [Header("彈性跟隨")]
    [SerializeField] private float springSpeed = 8f;

    [Header("拖曳縮放")]
    [SerializeField] private float dragZoomAmount = 1.5f;
    [SerializeField] private float zoomReturnSpeed = 5f;

    [Header("世界邊界 (回繞)")]
    [SerializeField] private float worldWidth = 100f;
    [SerializeField] private float worldHeight = 100f;

    [Header("方向線視覺")]
    [SerializeField] private Color lineColor = new(0.5f, 0.85f, 1f, 0.8f);
    [SerializeField] private float lineWidth = 0.08f;

    private bool _isDragging;
    private Vector2 _dragScreenOrigin;
    private Vector2 _dragDirection;
    private float _dragSpeedRatio;

    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private float _baseOrthoSize;

    private LineRenderer _lineRenderer;

    private void Awake()
    {
        CreateLineRenderer();
    }

    private void Start()
    {
        _startPosition = transform.position;
        _targetPosition = transform.position;
        _baseOrthoSize = Camera.main.orthographicSize;
    }

    /// <summary>回到初始位置。</summary>
    public void ReturnToStart()
    {
        _isDragging = false;
        _targetPosition = _startPosition;
        transform.position = _startPosition;
    }

    private void Update()
    {
        UpdateDragState();

        if (_isDragging)
            HandleDragMove();
        else
            HandleKeyboardMove();

        // 彈性跟隨：camera 平滑追 _targetPosition
        WrapTarget();
        transform.position = Vector3.Lerp(transform.position, _targetPosition, springSpeed * Time.deltaTime);

        // 拖曳時輕微拉近鏡頭，停止後復原
        float targetSize = _isDragging && _dragSpeedRatio > 0.01f
            ? _baseOrthoSize - dragZoomAmount * _dragSpeedRatio
            : _baseOrthoSize;
        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, targetSize, zoomReturnSpeed * Time.deltaTime);

        UpdateDragLine();
    }

    private void HandleKeyboardMove()
    {
        Vector2 input = ReadMoveInput();
        if (input.sqrMagnitude < 0.01f) return;

        Vector3 delta = new Vector3(input.x, input.y, 0f) * (moveSpeed * Time.deltaTime);
        _targetPosition += delta;
    }

    private void HandleDragMove()
    {
        if (_dragSpeedRatio < 0.01f) return;

        Vector3 dir = new Vector3(_dragDirection.x, _dragDirection.y, 0f);
        _targetPosition += dir * (moveSpeed * _dragSpeedRatio * Time.deltaTime);
    }

    private void UpdateDragState()
    {
        bool hasPointer = Mouse.current != null || (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed);
        if (!hasPointer && !_isDragging)
            return;

        bool pressed = false;
        bool released = false;
        Vector2 pointerPos = Vector2.zero;

        // 觸控優先（手機），fallback 到滑鼠（桌面）
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            pressed = Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
            released = false;
            pointerPos = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
        {
            released = true;
        }
        else if (Mouse.current != null)
        {
            pressed = Mouse.current.leftButton.wasPressedThisFrame;
            released = Mouse.current.leftButton.wasReleasedThisFrame;
            pointerPos = Mouse.current.position.ReadValue();
        }

        if (pressed)
        {
            _isDragging = true;
            _dragScreenOrigin = pointerPos;
            _dragDirection = Vector2.zero;
            _dragSpeedRatio = 0f;
        }

        if (_isDragging && !released)
        {
            Vector2 offset = pointerPos - _dragScreenOrigin;
            float dist = offset.magnitude;

            _dragSpeedRatio = Mathf.Clamp01(dist / dragMaxDistance);
            _dragDirection = dist > 1f ? offset.normalized : Vector2.zero;
        }

        if (released && _isDragging)
        {
            _isDragging = false;
        }
    }

    private void CreateLineRenderer()
    {
        var go = new GameObject("DragLine");
        go.transform.SetParent(transform);
        _lineRenderer = go.AddComponent<LineRenderer>();
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.startWidth = lineWidth;
        _lineRenderer.endWidth = lineWidth * 0.4f;
        _lineRenderer.positionCount = 2;
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.startColor = lineColor;
        _lineRenderer.endColor = lineColor;
        _lineRenderer.sortingOrder = 10;
        _lineRenderer.enabled = false;
    }

    private void UpdateDragLine()
    {
        if (!_isDragging || _dragSpeedRatio < 0.01f)
        {
            _lineRenderer.enabled = false;
            return;
        }

        _lineRenderer.enabled = true;

        Camera cam = Camera.main;
        Vector3 startWorld = cam.ScreenToWorldPoint(
            new Vector3(_dragScreenOrigin.x, _dragScreenOrigin.y, -cam.transform.position.z));
        startWorld.z = 0f;

        Vector2 pointerPos = GetPointerPosition();
        Vector3 endWorld = cam.ScreenToWorldPoint(
            new Vector3(pointerPos.x, pointerPos.y, -cam.transform.position.z));
        endWorld.z = 0f;

        _lineRenderer.SetPosition(0, startWorld);
        _lineRenderer.SetPosition(1, endWorld);
    }

    private static Vector2 ReadMoveInput()
    {
        if (Keyboard.current == null)
            return Vector2.zero;

        Vector2 input = Vector2.zero;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y -= 1f;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1f;

        return input.normalized;
    }

    private void WrapTarget()
    {
        // 不包裹鏡頭位置 — 鏡頭自由移動
        // PlanetFactory.LateUpdate 會把星星 wrap 到鏡頭附近
        // 這樣 Lerp 永遠走最短路徑，跨象限完全平順
    }

    private static Vector2 GetPointerPosition()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            return Touchscreen.current.primaryTouch.position.ReadValue();
        if (Mouse.current != null)
            return Mouse.current.position.ReadValue();
        return Vector2.zero;
    }
}
