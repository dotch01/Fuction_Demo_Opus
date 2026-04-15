using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Top-down 玩家控制器：WASD/方向鍵移動，帶簡單動畫回饋。
/// 附帶 Rigidbody2D + CircleCollider2D 用於碰撞偵測。
/// </summary>
public class Demo2PlayerController : MonoBehaviour
{
    public static Demo2PlayerController Instance { get; private set; }

    [Header("移動")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float acceleration = 40f;
    [SerializeField] private float deceleration = 30f;

    [Header("視覺")]
    [SerializeField] private float tiltAngle = 12f;
    [SerializeField] private float tiltSpeed = 10f;
    [SerializeField] private float squashAmount = 0.05f;

    private Rigidbody2D _rb;
    private Vector2 _inputDir;
    private Vector2 _velocity;
    private Transform _spriteRoot;

    public Vector2 Position => _rb != null ? _rb.position : (Vector2)transform.position;
    public bool IsMoving => _velocity.sqrMagnitude > 0.1f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Init(Transform spriteRoot)
    {
        _spriteRoot = spriteRoot;
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        ReadInput();
        UpdateVisual();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }

    private void ReadInput()
    {
        if (Keyboard.current == null) { _inputDir = Vector2.zero; return; }

        Vector2 raw = Vector2.zero;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) raw.x -= 1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) raw.x += 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) raw.y -= 1f;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) raw.y += 1f;

        _inputDir = raw.normalized;
    }

    private void ApplyMovement()
    {
        if (_inputDir.sqrMagnitude > 0.01f)
        {
            _velocity = Vector2.MoveTowards(_velocity, _inputDir * moveSpeed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            _velocity = Vector2.MoveTowards(_velocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
        }

        _rb.MovePosition(_rb.position + _velocity * Time.fixedDeltaTime);
    }

    private void UpdateVisual()
    {
        if (_spriteRoot == null) return;

        // 往移動方向微傾
        float targetTilt = -_velocity.x / moveSpeed * tiltAngle;
        float currentZ = _spriteRoot.localEulerAngles.z;
        if (currentZ > 180f) currentZ -= 360f;
        float newZ = Mathf.Lerp(currentZ, targetTilt, tiltSpeed * Time.deltaTime);
        _spriteRoot.localEulerAngles = new Vector3(0, 0, newZ);

        // 移動時微微壓扁（像果凍）
        float speed01 = Mathf.Clamp01(_velocity.magnitude / moveSpeed);
        float sx = 1f + squashAmount * speed01;
        float sy = 1f - squashAmount * speed01;
        _spriteRoot.localScale = new Vector3(sx, sy, 1f);
    }
}
